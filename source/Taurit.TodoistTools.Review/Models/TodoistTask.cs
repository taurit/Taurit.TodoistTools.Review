namespace Taurit.TodoistTools.Review.Models;

public sealed record TodoistTask(string Id,
    string Content,
    string Description,
    int Priority,
    List<Label> Labels
);
