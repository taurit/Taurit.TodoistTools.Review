using System;
using System.Collections.Generic;

namespace TodoistReview.Models.TodoistApiModels
{
    /// <summary>
    ///     Fake repository for development purposes, so the real API is not called too many times
    /// </summary>
    public class FakeTaskRepository : ITaskRepository
    {
        private string syncKey;

        public FakeTaskRepository(string syncKey)
        {
            this.syncKey = syncKey;
        }

        public IList<Label> GetAllLabels()
        {
            var labels = new List<Label>();

            labels.Add(new Label(1, 0, "laptop"));
            labels.Add(new Label(2, 0, "mobile"));
            labels.Add(new Label(3, 0, "market"));

            return labels;
        }

        public IList<TodoTask> GetAllTasks()
        {
            var tasks = new List<TodoTask>();
            
            // tasks with one label
            tasks.Add(new TodoTask {id = 1, content = "Update software", labels = new List<Int64> {1}});
            tasks.Add(new TodoTask {id = 2, content = "Sync podcasts when on Wifi", labels = new List<Int64> {2}});
            tasks.Add(new TodoTask {id = 3, content = "Buy milk", labels = new List<Int64> {3}});

            // task with two labels
            tasks.Add(new TodoTask
            {
                id = 3,
                content = "Find and read reviews of my book",
                labels = new List<Int64> {1, 2}
            });

            // task with no labels
            tasks.Add(new TodoTask { id = 4, content = "Task 5", labels = new List<Int64>() });
            tasks.Add(new TodoTask { id = 5, content = "Task 6", labels = new List<Int64>() });

            return tasks;
        }

        public String UpdateTasks(List<TodoTask> tasksToUpdate)
        {
            // fake
            return "I'm a fake object and I did nothing";
        }
    }
}