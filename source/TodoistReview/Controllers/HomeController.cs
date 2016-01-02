using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TodoistReview.Models;
using TodoistReview.Models.TodoistApiModels;

namespace TodoistReview.Controllers
{
    public class HomeController : Controller
    {
        private const string SyncCookieName = "SyncApiCookie2";
        //ITaskRepository repository = new FakeTaskRepository();

        // GET: Home
        public ActionResult Index()
        {
            if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
                return RedirectToAction("Login");

            return View();    
        }

        public ActionResult Login(TodoistAuthData authData)
        {
            if (authData != null && !String.IsNullOrWhiteSpace(authData.APIToken))
            {
                this.ControllerContext.HttpContext.Response.SetCookie(new System.Web.HttpCookie(SyncCookieName, authData.APIToken));
                return RedirectToAction("Index");
            }
            return View("Login");
        }

        public JsonResult GetAllLabels()
        {
            if (!ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
                throw new InvalidOperationException("Authorization cookie not found");

            string syncKey = this.ControllerContext.HttpContext.Request.Cookies[SyncCookieName].Value;
            TodoistTaskRepository repository = new TodoistTaskRepository(syncKey);

            List<Label> labels = repository.GetAllLabels().OrderBy(label => label.item_order).ToList();

            return Json(labels, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllTasks()
        {
            if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
                throw new InvalidOperationException("Authorization cookie not found");
            string syncKey = this.ControllerContext.HttpContext.Request.Cookies[SyncCookieName].Value;
            TodoistTaskRepository repository = new TodoistTaskRepository(syncKey);

            List<TodoTask> tasks = repository.GetAllTasks().ToList();

            // review only those which have no labels (contexts) or have more than 1 label
            tasks = tasks.Where(task => task.labels != null && task.labels.Count != 1)
                .Take(15) // batch size
                .ToList();

            foreach (var task in tasks)
            {
                task.originalLabels = task.labels;
            }

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTasks(List<TodoTask> tasks)
        {
            if (!this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(SyncCookieName))
                throw new InvalidOperationException("Authorization cookie not found");
            string syncKey = this.ControllerContext.HttpContext.Request.Cookies[SyncCookieName].Value;
            TodoistTaskRepository repository = new TodoistTaskRepository(syncKey);

            List<TodoTask> tasksToUpdate = tasks.Where(task => task.LabelsDiffer).ToList();
            repository.UpdateTasks(tasksToUpdate);

            return Json(tasksToUpdate.Count);
        }


    }
}