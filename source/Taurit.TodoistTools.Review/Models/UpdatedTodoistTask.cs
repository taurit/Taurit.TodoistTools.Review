namespace Taurit.TodoistTools.Review.Models;

public sealed record UpdatedTodoistTask(
    TodoistTask OriginalTodoistTask,
    List<Label> NewLabels,
    int NewPriority,
    string NewContent,
    string NewDescription
);
