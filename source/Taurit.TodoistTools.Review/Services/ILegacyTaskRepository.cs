using Taurit.TodoistTools.Review.Models.TodoistSyncV8;

namespace Taurit.TodoistTools.Review.Services;

[Obsolete("Use newer ITaskRepository. This one is sensitive to changes in Todoist contract, and contains more than needed. Won't work after 2022-11-01.")]
internal interface ILegacyTaskRepository
{
    Task<IList<Label>> GetAllLabels();
    Task<IList<TodoTask>> GetAllTasks();
    Task<string> UpdateTasks(List<TodoTask> tasksToUpdate);
}
