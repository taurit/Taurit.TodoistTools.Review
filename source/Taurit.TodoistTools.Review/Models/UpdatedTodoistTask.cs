namespace Taurit.TodoistTools.Review.Models;

public sealed record UpdatedTodoistTask(
    TodoistTask OriginalTask,
    List<string> Labels,
    int Priority,
    string Content,
    string Description
);
