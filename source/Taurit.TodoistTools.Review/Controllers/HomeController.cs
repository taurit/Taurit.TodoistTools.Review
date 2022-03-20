﻿using System.Diagnostics.CodeAnalysis;
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

    private readonly MultiCultureTimespanParser _timespanParser;

#pragma warning disable CS8618
    public HomeController(ILogger<HomeController> logger)
#pragma warning restore CS8618
    {
        _logger = logger;
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
#if DEBUG
            _repository = new FakeTaskRepository(todoistApiKey);
#else
            _repository = new TodoistTaskRepository(todoistApiKey);
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
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
        }

        var allLabels = await _repository.GetAllLabels();
        List<Label> labels = allLabels.OrderBy(label => label.item_order)
            .Union(Label.SpecialLabels)
            .ToList();

        return Json(labels);
    }

    public async Task<JsonResult> GetTasksToReview()
    {
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
        }

        IList<TodoTask> allTasks = (await _repository.GetAllTasks());
        List<TodoTask> tasks = allTasks.ToList();

        // parse estimated time in a natural language
        foreach (TodoTask todoTask in tasks)
        {
            if (todoTask.content is not null)
            {
                TimespanParseResult parsedDuration = _timespanParser.Parse(todoTask.content);

                if (parsedDuration.Success)
                {
                    todoTask.SetOriginalDurationInMinutes((Int32)parsedDuration.Duration.TotalMinutes);
                }
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
        var filteredTasks = tasks.Where(TaskNeedsReview)
            .Take(7) // batch size - only this much tasks will be passed to the client side (browser), and only this much tasks will be updated via the Todoist API in a single update request
            //.TakeLast(2) // debug only
            .ToList();
        return filteredTasks;
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
    public async Task<JsonResult> UpdateTasks([FromBody] List<TodoTask> tasks)
    {
        if (TryGetTodoistApiKeyForCurrentRequest() is null)
        {
            throw new InvalidOperationException("Todoist API Key was not found in cookie nor env variable");
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

        var response = await _repository.UpdateTasks(changedTasks);
        _logger.LogInformation("Received response from Todoist Sync API: {Response}", response);

        // hack: perhaps the error was with not awaiting the task, but my another suspicion is that back-end has some eventual consistency model. And if the front-end queries too soon, it appeared to confusingly returned the same tasks again, appearing unreviewed. This is supposed to mitigate this from happening:
        await Task.Delay(TimeSpan.FromSeconds(3));

        return Json(changedTasks.Count);
    }
}
