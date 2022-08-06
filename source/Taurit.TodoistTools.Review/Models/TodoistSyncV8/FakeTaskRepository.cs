using Taurit.TodoistTools.Review.Services.TodoistSyncV8;

namespace Taurit.TodoistTools.Review.Models.TodoistSyncV8;

/// <summary>
///     Fake repository for development purposes, so the real API is not called too many times
/// </summary>
[Obsolete("Works only with deprecated Todoist Sync API v8")]
public class FakeTaskRepository : ILegacyTaskRepository
{
    // ReSharper disable once UnusedParameter.Local
    public FakeTaskRepository(String syncKey)
    {
    }

    public Task<IList<Label>> GetAllLabels()
    {
        List<Label>? labels = new List<Label>
        {
            new Label(1, 0, "laptop"),
            new Label(2, 0, "mobile"),
            new Label(3, 0, "market")
        };

        return Task.FromResult((IList<Label>)labels);
    }

    public Task<IList<TodoTask>> GetAllTasks()
    {
        List<TodoTask>? tasks = new List<TodoTask>
        {

            // tasks with one label
            new TodoTask { id = 1, priority = 1, content = "Update software", labels = new List<Int64> { 1 }, description = "Avoid non-stable versions" },
            new TodoTask { id = 2, priority = 2, content = "Sync podcasts when on Wifi", labels = new List<Int64> { 2 }, description = "Don't re-add addictive crap" },
            new TodoTask { id = 3, priority = 3, content = "Buy milk", labels = new List<Int64> { 3 }, description = "Preferably made from plants" },

            // task with two labels
            new TodoTask
            {
                id = 3,
                priority = 1,
                content = "Find and read reviews of my book",
                labels = new List<Int64> { 1, 2 }
            },

            // task with no labels
            new TodoTask { id = 4, priority = 2, content = "Task 5 (5 min)", labels = new List<Int64>() },
            new TodoTask { id = 5, priority = 4, content = "Task 6 high priority", labels = new List<Int64>() }
        };

        return Task.FromResult((IList<TodoTask>)tasks);
    }

    public Task<String> UpdateTasks(List<TodoTask> tasksToUpdate)
    {
        // fake
        return Task.FromResult("I'm a fake object and I did nothing");
    }
}
