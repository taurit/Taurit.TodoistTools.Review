namespace Taurit.TodoistTools.Review.Models.TodoistSyncV9;

public record TodoistTask(
    string Id,
    string Content,
    string Description,
    int Priority,
    List<string> Labels);
