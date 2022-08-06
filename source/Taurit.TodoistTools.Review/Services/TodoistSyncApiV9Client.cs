using Taurit.TodoistTools.Review.Models;

namespace Taurit.TodoistTools.Review.Services;

internal class TodoistSyncApiV9Client : ITodoistApiClient
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

    public Task UpdateTasks(List<UpdatedTodoistTask> changedTasks)
    {
        throw new NotImplementedException();
    }

    Task<List<Label>> ITodoistApiClient.GetAllLabels()
    {
        throw new NotImplementedException();
    }
}
