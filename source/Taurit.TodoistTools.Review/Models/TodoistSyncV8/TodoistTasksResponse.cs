// ReSharper disable InconsistentNaming - names reflect those in API

namespace Taurit.TodoistTools.Review.Models.TodoistSyncV8;

public class TodoistTasksResponse
{
    public Int64 seq_no_global { get; set; }
    public List<TodoTask>? Items { get; set; }

    public Int64 UserId { get; set; }
}
