using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NaturalLanguageTimespanParser;
using System.Globalization;
using Taurit.TodoistTools.Review.Models;
using Taurit.TodoistTools.Review.Models.TodoistApiModels;

namespace Taurit.TodoistTools.Review.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private const String SyncCookieName = "SyncApiCookie2";

    private ITaskRepository _repository;

    private MultiCultureTimespanParser _timespanParser;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    /// <remarks>
    ///     reading cookie can't be done in constructor as ControllerContext is still null there
    /// </remarks>
    public override void OnActionExecuting(ActionExecutingContext ctx)
    {
        base.OnActionExecuting(ctx);
        String? syncKey = ControllerContext.HttpContext.Request.Cookies[SyncCookieName];
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

    public async Task<JsonResult> GetAllLabels()
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            throw new InvalidOperationException("Authorization cookie not found");
        }

        var allLabels = await _repository.GetAllLabels();
        List<Label> labels = allLabels.OrderBy(label => label.item_order)
            .Union(Label.SpecialLabels)
            .ToList();

        return Json(labels);
    }

    public async Task<JsonResult> GetTasksToReview()
    {
        if (!ControllerContext.HttpContext.Request.Cookies.ContainsKey(SyncCookieName))
        {
            throw new InvalidOperationException("Authorization cookie not found");
        }

        IList<TodoTask> allTasks = (await _repository.GetAllTasks());
        List<TodoTask> tasks = allTasks.ToList();

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
        tasks = tasks.Where(TaskNeedsReview)
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

        List<TodoTask> changedTasks = tasks.Where(task => task.ItemWasChangedByUser).ToList();
        List<TodoTask> unchangedTasks = tasks.Where(task => !task.ItemWasChangedByUser).ToList();

        _logger.LogInformation(
            "App is about to POST a batch of updates. {NumUpdatedTasks} tasks updated by user, {NumNotUpdatedTasks} not updated",
            changedTasks.Count, unchangedTasks.Count);
        foreach (var unchangedTask in unchangedTasks)
        {
            _logger.LogWarning("A task (id={TaskId}) was apparently unchanged by user, which is unusual. Old content: {OldContent}, new content: {NewContent}", unchangedTask.id, unchangedTask.originalContent, unchangedTask.content);
        }

        _repository.UpdateTasks(changedTasks);

        return Json(changedTasks.Count);
    }
}
