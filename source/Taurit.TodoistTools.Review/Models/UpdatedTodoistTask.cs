namespace Taurit.TodoistTools.Review.Models;

public sealed record UpdatedTodoistTask(
    TodoistTask OriginalTask,
    List<string> Labels,
    int Priority,
    string Content,
    string Description,
    int EstimatedTimeMinutes
)
{
    public Boolean ItemWasChangedByUser
    {
        get
        {
            var contentChanged = this.Content != OriginalTask.Content;
            var labelsChanged = !this.Labels.OrderBy(x => x).SequenceEqual(OriginalTask.Labels.Select(x => x.Name).OrderBy(x => x));
            var priorityChanged = Priority != OriginalTask.Priority;
            var descriptionChanged = Description != OriginalTask.Description;
            var estimatedTimeChanged = EstimatedTimeMinutes != OriginalTask.EstimatedTimeMinutes;

            return contentChanged || labelsChanged || priorityChanged || descriptionChanged || estimatedTimeChanged;
        }
    }
}
