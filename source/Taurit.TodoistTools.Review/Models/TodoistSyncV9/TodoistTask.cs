using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Taurit.TodoistTools.Review.Models.TodoistSyncV9;

public enum DurationUnit { minute, day }

[Serializable]
[DebuggerDisplay("{amount} {unit}")]
public class Duration
{
    public int amount { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DurationUnit unit { get; init; }

}

[Serializable]
public record TodoistTask(
    string Id,
    string Content,
    string Description,
    int Priority,
    List<string> Labels,
    Duration? duration
    );
