using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NaturalLanguageTimespanParser;
using System.Globalization;
using Taurit.TodoistTools.Review.Models;
using Taurit.TodoistTools.Review.Models.TodoistApiModels;

namespace Taurit.TodoistTools.Review.Controllers;

public class HomeController : Controller
{
    private const String SyncCookieName = "SyncApiCookie2";

    private ITaskRepository _repository;

    private MultiCultureTimespanParser _timespanParser;

    /// <remarks>
    ///     reading cookie can't be done in constructor as ControllerContext is still null there
    /// </remarks>
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        base.OnActionExecuting(ctx);
        String syncKey = ControllerContext?.HttpContext.Request.Cookies[SyncCookieName];
        if (syncKey != null)
        {
#if DEBUG
            _repository = new FakeTaskRepository(syncKey);
#else
                _repository = new TodoistTaskRepository(syncKey);
#endif
        }

        _timespanParser = new MultiCultureTimespanParser(new[] { new CultureInfo("pl"), new CultureInfo("en") });
    }

    // GET: Home
    public ActionResult Index()
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            return RedirectToAction("Login");
        }

        return View();
    }

    public ActionResult Login(TodoistAuthData authData)
    {
        if (authData != null && !String.IsNullOrWhiteSpace(authData.ApiToken))
        {
            ControllerContext.HttpContext.Response.Cookies.Append(SyncCookieName, authData.ApiToken, new CookieOptions()
            {
                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS.
                Secure = true,

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                HttpOnly = true,

                SameSite = SameSiteMode.Strict,

                Expires = DateTime.Now.AddDays(360)
            });
            return RedirectToAction("Index");
        }
        return View("Login");
    }

    public JsonResult GetAllLabels()
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            throw new InvalidOperationException("Authorization cookie not found");
        }

        List<Label> labels = _repository.GetAllLabels().OrderBy(label => label.item_order)
            .Union(Label.SpecialLabels)
            .ToList();

        return Json(labels);
    }

    public JsonResult GetTasksToReview()
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            throw new InvalidOperationException("Authorization cookie not found");
        }
        List<TodoTask> tasks = _repository.GetAllTasks().ToList();

        // parse estimated time in a natural language
        foreach (TodoTask todoTask in tasks)
        {
            TimespanParseResult parsedDuration = _timespanParser.Parse(todoTask.content);

            if (parsedDuration.Success)
            {
                todoTask.SetOriginalDurationInMinutes((Int32)parsedDuration.Duration.TotalMinutes);
            }
        }

        foreach (TodoTask task in tasks)
        {
            task.SaveOriginalValues();
        }

        tasks = FilterTasksAndReturnOnlyOnesThatNeedReview(tasks);

        return Json(tasks);
    }

    private static List<TodoTask> FilterTasksAndReturnOnlyOnesThatNeedReview(List<TodoTask> tasks)
    {
        tasks = tasks.Where(task => TaskNeedsReview(task))
            .Take(12) // batch size - only this much tasks will be passed to the client side (browser), and only this much tasks will be updated via the Todoist API in a single update request
            .ToList();
        return tasks;
    }

    private static Boolean TaskNeedsReview(TodoTask task)
    {
        // Assumptions:
        // * we want to have exactly 1 label/context assigned after the review, so tasks with 0 or 2+ labels needs to be re-reviewed
        // * there's no point in reviewing and updating metadata of tasks that are already deleted or done
        // * we don't want tasks with a default priority (1) - reviewed task should have a priority of 2, 3, or 4 assigned (low, medium or high)
        bool labelsNeedReview = task.labels != null &&
                               task.labels.Count != 1;
        bool estimatedTimeNeedsReview = !task.timeEstimateWasAlreadyDefinedOnTheServerSide;
        bool priorityNeedsReview = task.priority == 1;
        bool taskIsNotCompletedYet = task.is_deleted == 0 &&
                                    task.@checked == 0;

        bool taskNeedsReview = (labelsNeedReview || estimatedTimeNeedsReview || priorityNeedsReview)
                              && taskIsNotCompletedYet;
        return taskNeedsReview;
    }

    [HttpPost]
    public JsonResult UpdateTasks([FromBody] List<TodoTask> tasks)
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            throw new InvalidOperationException("Authorization cookie not found");
        }

        List<TodoTask> tasksToUpdate = tasks.Where(task => task.ItemWasChangedByUser).ToList();
        _repository.UpdateTasks(tasksToUpdate);

        return Json(tasksToUpdate.Count);
    }
}
