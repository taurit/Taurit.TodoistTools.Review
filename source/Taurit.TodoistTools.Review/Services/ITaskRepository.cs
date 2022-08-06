using Taurit.TodoistTools.Review.Models.TodoistSyncV9;

namespace Taurit.TodoistTools.Review.Services;

internal interface ITaskRepository
{
    Task<IList<Label>> GetAllLabels();
    Task<IList<TodoTask>> GetAllTasks();
    Task<string> UpdateTasks(List<TodoTask> tasksToUpdate);
}
