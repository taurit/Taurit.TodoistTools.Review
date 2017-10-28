using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TodoistReview.Models;
using TodoistReview.Models.TodoistApiModels;

namespace TodoistReview.Controllers
{
    public class HomeController : Controller
    {
        private const String SyncCookieName = "SyncApiCookie2";
        //ITaskRepository repository = new FakeTaskRepository();

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
                ControllerContext.HttpContext.Response.SetCookie(new HttpCookie(SyncCookieName, authData.ApiToken));
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

            String syncKey = ControllerContext.HttpContext.Request.Cookies[SyncCookieName]?.Value;
            var repository = new FakeTaskRepository(syncKey);

            List<Label> labels = repository.GetAllLabels().OrderBy(label => label.item_order)
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
            String syncKey = ControllerContext.HttpContext.Request.Cookies[SyncCookieName]?.Value;
            //var repository = new TodoistTaskRepository(syncKey);
            var repository = new FakeTaskRepository(syncKey);

            List<TodoTask> tasks = repository.GetAllTasks().ToList();

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
            String syncKey = ControllerContext.HttpContext.Request.Cookies[SyncCookieName]?.Value;
            var repository = new FakeTaskRepository(syncKey);

            List<TodoTask> tasksToUpdate = tasks.Where(task => task.ItemWasChangedByUser).ToList();
            repository.UpdateTasks(tasksToUpdate);

            return Json(tasksToUpdate.Count);
        }
    }
}