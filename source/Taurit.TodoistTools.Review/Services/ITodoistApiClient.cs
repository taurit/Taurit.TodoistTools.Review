using Taurit.TodoistTools.Review.Models;

namespace Taurit.TodoistTools.Review.Services;

internal interface ITodoistApiClient
{
    Task<List<Label>> GetAllLabels();
    Task<List<TodoistTask>> GetAllTasks();
    Task UpdateTasks(List<UpdatedTodoistTask> changedTasks);
}
