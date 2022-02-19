namespace Taurit.TodoistTools.Review.Models;

internal interface ITaskRepository
{
    Task<IList<Label>> GetAllLabels();
    Task<IList<TodoTask>> GetAllTasks();
    Task<String> UpdateTasks(List<TodoTask> tasksToUpdate);
}
