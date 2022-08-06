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
            Boolean contentChanged = Content != OriginalTask.Content;
            Boolean labelsChanged = !Labels.OrderBy(x => x)
                .SequenceEqual(OriginalTask.Labels.Select(x => x.Name).OrderBy(x => x));
            Boolean priorityChanged = Priority != OriginalTask.Priority;
            Boolean descriptionChanged = Description != OriginalTask.Description;
            Boolean estimatedTimeChanged = EstimatedTimeMinutes != OriginalTask.EstimatedTimeMinutes;

            return contentChanged || labelsChanged || priorityChanged || descriptionChanged || estimatedTimeChanged;
        }
    }

    public string ContentWithTimeMetadata
    {
        get
        {
            if (OriginalTask.EstimatedTimeMinutes != 0)
            {
                return Content;
            }

            if (OriginalTask.EstimatedTimeMinutes != 0 && EstimatedTimeMinutes != OriginalTask.EstimatedTimeMinutes)
            {
                return
                    Content; // not supported yet - update is a bit difficult (time string needs to be replaced with another). Requires good tests not to break the content
            }

            if (EstimatedTimeMinutes == 0)
            {
                return Content; // 0 is not a valid time estimate, don't save it
            }

            string newContent = $"{Content} ({EstimatedTimeMinutes} min)";

            return newContent;
        }
    }
}
