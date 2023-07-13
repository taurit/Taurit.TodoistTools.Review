using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using NaturalLanguageTimespanParser;
using System.Globalization;
using Taurit.TodoistTools.Review.Models;
using Taurit.TodoistTools.Review.Services;

namespace Taurit.TodoistTools.Review.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private const String SyncCookieName = "SyncApiCookie2";

    private ITodoistApiClient _todoistApiClient;

    private readonly MultiCultureTimespanParser _timespanParser;

#pragma warning disable CS8618
    public HomeController(HttpClient httpClient)
#pragma warning restore CS8618
    {
        _httpClient = httpClient;
        _timespanParser = new MultiCultureTimespanParser(new[] { new CultureInfo("pl"), new CultureInfo("en") });
    }

    /// <remarks>
    ///     Reading cookie can't be done in constructor as ControllerContext is still null there
    /// </remarks>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        String? todoistApiKey = TryGetTodoistApiKeyForCurrentRequest();
        if (todoistApiKey != null)
        {
#if !DEBUG
            _todoistApiClient = new FakeTodoistApiClient(todoistApiKey);
#else
            _todoistApiClient = new TodoistSyncApiV9Client(todoistApiKey, _httpClient, _timespanParser);
#endif
        }
    }

    private string? TryGetTodoistApiKeyForCurrentRequest()
    {
        // From cookie (assuming multi-user mode, hosted in Azure)
        String? apiKeyFromCookie = ControllerContext.HttpContext.Request.Cookies[SyncCookieName];

        // From env variable (assuming local Docker instance for my own usage)
        String? apiKeyFromEnv = Environment.GetEnvironmentVariable("TodoistApiKey");

        return apiKeyFromCookie ?? apiKeyFromEnv;
    }

    // GET: Home
    public ActionResult Index()
    {
        var apiKey = TryGetTodoistApiKeyForCurrentRequest();
        if (apiKey is null)
        {
            return RedirectToAction("Login");
        }

        return View();
    }

    public ActionResult Login(TodoistAuthenticationData authenticationData)
    {
        if (authenticationData != null && !String.IsNullOrWhiteSpace(authenticationData.ApiToken))
        {
            ControllerContext.HttpContext.Response.Cookies.Append(SyncCookieName, authenticationData.ApiToken, new CookieOptions
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
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
        }

        List<Label> labels = await _todoistApiClient.GetAllLabels();
        labels.Add(new Label("eliminate"));
        return Json(labels);
    }

    public async Task<JsonResult> GetTasksToReview()
    {
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
        }

        IList<TodoistTask> allTasks = (await _todoistApiClient.GetAllTasks());
        List<TodoistTask> tasks = allTasks.ToList();
        var tasksThatNeedReview = tasks.Where(TaskNeedsReview)
            //.TakeLast(2).ToList(); // debug
            .Take(7).ToList();

        return Json(tasksThatNeedReview);
    }

    private static Boolean TaskNeedsReview(TodoistTask task)
    {
        // Assumptions:
        // * we want to have exactly 1 label/context assigned after the review, so tasks with 0 or 2+ labels needs to be re-reviewed
        // * there's no point in reviewing and updating metadata of tasks that are already deleted or done
        // * we don't want tasks with a default priority (1) - reviewed task should have a priority of 2, 3, or 4 assigned (low, medium or high)
        bool labelsNeedReview = task.Labels.Count != 1;
        bool estimatedTimeNeedsReview = task.EstimatedTimeMinutes == 0;
        bool priorityNeedsReview = task.Priority == 1;

        bool taskNeedsReview = labelsNeedReview || estimatedTimeNeedsReview || priorityNeedsReview;
        return taskNeedsReview;
    }

    [HttpPost]
    public async Task<IStatusCodeActionResult> UpdateTasks([FromBody] List<UpdatedTodoistTask> tasks)
    {
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
        }

        List<UpdatedTodoistTask> changedTasks = tasks.Where(task => task.ItemWasChangedByUser).ToList();

        await _todoistApiClient.UpdateTasks(changedTasks);

        return Ok();
    }
}
