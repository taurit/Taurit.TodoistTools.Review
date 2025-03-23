namespace Taurit.TodoistTools.Review.Models;

public sealed record UpdatedTodoistTask(
    TodoistTask OriginalTask,
    List<string> Labels,
    int Priority,
    string Content,
    string Description
)
{
    public Boolean ItemWasChangedByUser
    {
        get
        {
            Boolean contentChanged = Content != OriginalTask.Content;
            Boolean labelsChanged = !Labels.OrderBy(x => x)
                .SequenceEqual(OriginalTask.Labels.Select(x => x.Name).OrderBy(x => x));
            Boolean priorityChanged = Priority != OriginalTask.Priority;
            Boolean descriptionChanged = Description != OriginalTask.Description;

            return contentChanged || labelsChanged || priorityChanged || descriptionChanged;
        }
    }

}
