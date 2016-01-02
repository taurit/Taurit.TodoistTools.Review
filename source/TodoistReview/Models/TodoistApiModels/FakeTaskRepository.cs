using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models.TodoistApiModels
{
    /// <summary>
    /// Fake repository for development purposes, so the real API is not called too many times
    /// </summary>
    public class FakeTaskRepository : ITaskRepository
    {
        public IList<Label> GetAllLabels()
        {
            List<Label> labels = new List<Label>();

            labels.Add(new Label() { id = 1, is_deleted = 0, name = "laptop" });
            labels.Add(new Label() { id = 2, is_deleted = 0, name = "mobile" });
            labels.Add(new Label() { id = 3, is_deleted = 0, name = "market" });

            return labels;
        }

        public IList<TodoTask> GetAllTasks()
        {
            List<TodoTask> tasks = new List<TodoTask>();

            // tasks with one label
            tasks.Add(new TodoTask() { id = 1, content = "Update software", labels = new List<long>() { 1 } });
            tasks.Add(new TodoTask() { id = 2, content = "Sync podcasts when on Wifi", labels = new List<long>() { 2 } });
            tasks.Add(new TodoTask() { id = 3, content = "Buy milk", labels = new List<long>() { 3 } });

            // task with two labels
            tasks.Add(new TodoTask() { id = 3, content = "Find and read reviews of my book", labels = new List<long>() { 1, 2 } });

            return tasks;
        }

        public string UpdateTasks(List<TodoTask> tasksToUpdate)
        {
            // fake
            return "I'm a fake object and I did nothing";
        }
    }
}