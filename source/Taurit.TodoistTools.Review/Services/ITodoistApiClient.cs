using Taurit.TodoistTools.Review.Models;

namespace Taurit.TodoistTools.Review.Services;

internal interface ITodoistApiClient
{
    Task<List<Label>> GetAllLabels();
    Task<List<TodoistTask>> GetAllTasks();
}

class TodoistSyncApiV9Client : ITodoistApiClient
{
    public TodoistSyncApiV9Client(String? todoistApiKey)
    {
        throw new NotImplementedException();
    }

    public Task<Label> GetAllLabels()
    {
        throw new NotImplementedException();
    }

    public async Task<List<TodoistTask>> GetAllTasks()
    {
        //if (todoTask.Content is not null)
        //{
        //    TimespanParseResult parsedDuration = _timespanParser.Parse(todoTask.Content);

        //    if (parsedDuration.Success)
        //    {
        //        todoTask.SetOriginalDurationInMinutes((Int32)parsedDuration.Duration.TotalMinutes);
        //    }
        //}
        return new List<TodoistTask>();
    }

    Task<List<Label>> ITodoistApiClient.GetAllLabels()
    {
        throw new NotImplementedException();
    }
}

#pragma warning disable CS1998
internal class FakeTodoistApiClient : ITodoistApiClient
{
    public FakeTodoistApiClient(String todoistApiKey) { }

    public async Task<List<TodoistTask>> GetAllTasks()
    {
        return new List<TodoistTask>
        {
            new TodoistTask("task id 1", "Buy milk (mock task)", "Some description", 1, new List<Label>(), 0),
            new TodoistTask("task id 2", "Buy carrots (mock task)", "Some description 2", 2, new List<Label>(), 0)

        };
    }

    public async Task<List<Label>> GetAllLabels()
    {
        return new List<Label> { new Label("home"), new Label("pc"), new Label("mobile") };
    }
}
#pragma warning restore CS1998
