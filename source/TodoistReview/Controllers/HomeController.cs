using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NaturalLanguageTimespanParser;
using TodoistReview.Models;
using TodoistReview.Models.TodoistApiModels;

namespace TodoistReview.Controllers
{
    public class HomeController : Controller
    {
        private const String SyncCookieName = "SyncApiCookie2";

        private  ITaskRepository _repository;

        private MultiCultureTimespanParser _timespanParser;

        /// <remarks>
        ///     reading cookie can't be done in constructor as ControllerContext is still null there
        /// </remarks>
        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            String syncKey = ControllerContext?.HttpContext.Request.Cookies[SyncCookieName]?.Value;
            if (syncKey != null)
            {
#if DEBUG
                _repository = new FakeTaskRepository(syncKey);
#else
                _repository = new TodoistTaskRepository(syncKey);
#endif
            }

            _timespanParser = new MultiCultureTimespanParser(new[] {new CultureInfo("pl"), new CultureInfo("en")});
        }

        // GET: Home
        public ActionResult Index()
        {
            if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult Login(TodoistAuthData authData)
        {
            if (authData != null && !String.IsNullOrWhiteSpace(authData.ApiToken))
            {
                var loginCookie = new HttpCookie(SyncCookieName, authData.ApiToken);
                loginCookie.Expires = DateTime.Now.AddDays(360);
                loginCookie.HttpOnly = true;
                ControllerContext.HttpContext.Response.SetCookie(loginCookie);

                return RedirectToAction("Index");
            }
            return View("Login");
        }

        public JsonResult GetAllLabels()
        {
            if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
            {
                throw new InvalidOperationException("Authorization cookie not found");
            }

            List<Label> labels = _repository.GetAllLabels().OrderBy(label => label.item_order)
                .Union(Label.SpecialLabels)
                .ToList();

            return Json(labels, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTasksToReview()
        {
            if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
            {
                throw new InvalidOperationException("Authorization cookie not found");
            }
            List<TodoTask> tasks = _repository.GetAllTasks().ToList();
            
            // parse estimated time in a natural language
            foreach (TodoTask todoTask in tasks)
            {
                var parsedDuration = _timespanParser.Parse(todoTask.content);
                if (parsedDuration.Success)
                {
                    todoTask.time = (Int32) parsedDuration.Duration.TotalMinutes;
                }
            }

            // review only those which have no labels (contexts) or have more than 1 label
            // (assumption: we want to have exactly 1 label/context assigned after the review)
            tasks = tasks.Where(task => task.labels != null &&
                                        task.labels.Count != 1 &&
                                        task.is_deleted == 0 &&
                                        task.@checked == 0
                )
                .Take(15) // batch size
                .ToList();

            foreach (TodoTask task in tasks)
            {
                task.SaveOriginalValues();
            }

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTasks(List<TodoTask> tasks)
        {
            if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
            {
                throw new InvalidOperationException("Authorization cookie not found");
            }

            List<TodoTask> tasksToUpdate = tasks.Where(task => task.ItemWasChangedByUser).ToList();
            _repository.UpdateTasks(tasksToUpdate);

            return Json(tasksToUpdate.Count);
        }
    }
}