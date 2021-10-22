using Newtonsoft.Json;
using System.Diagnostics;

// ReSharper disable InconsistentNaming - names match those in documentation

namespace Taurit.TodoistTools.Review.Models;

/// <summary>
///     Field names as in: https://developer.todoist.com/#items
/// </summary>
[DebuggerDisplay("{" + nameof(content) + "}")]
public class TodoTask
{

    [JsonProperty]
    public Int64 id { get; set; }

    [JsonProperty]
    public String content { get; set; }

    [JsonProperty]
    public String description { get; set; }

    [JsonProperty]
    public String originalContent { get; set; }


    [JsonProperty]
    public bool timeEstimateWasAlreadyDefinedOnTheServerSide { get; set; }

    public string contentWithTime
    {
        get
        {
            Debug.Assert(originalTime >= 0);
            Debug.Assert(time >= 0);
            Debug.Assert(content != null);

            if (originalTime != 0) return content;
            if (originalTime != 0 && time != originalTime) return content; // not supported yet - update is a bit difficult (time string needs to be replaced with another). Requires good tests not to break the content
            if (timeEstimateWasAlreadyDefinedOnTheServerSide) return content; // same situation as above, but if time was defined as 0 min
            if (time == 0) return content; // 0 is not a valid time estimate, don't save it

            string? newContent = $"{content} ({(int)time} min)";

            return newContent;
        }
    }


    [JsonProperty]
    public List<Int64> labels { get; set; }

    /// <summary>
    ///     A copy of "labels" which should not be changed on the client side.
    ///     It helps to determine if user changed label collection for the task, and whether
    ///     calling update on Todoist API is needed.
    /// </summary>
    [JsonProperty]
    public List<Int64> originalLabels { get; set; }

    /// <summary>
    /// Estimated time for a task before user's changes
    /// </summary>
    [JsonProperty]
    public Int64 originalTime { get; private set; }

    /// <summary>
    /// Estimated time for a task
    /// </summary>
    [JsonProperty]
    public Int64 time { get; set; }

    /// <summary>
    ///     The priority of the task (a number between 1 and 4, 4 for very urgent and 1 for natural).
    /// </summary>
    [JsonProperty]
    public Int32 priority { get; set; }

    /// <summary>
    /// Initial priority value from the server, before user might have changed it
    /// </summary>
    [JsonProperty]
    public Int32 originalPriority { get; set; }

    [JsonProperty]
    public Int64 project_id { get; set; }

    /// <summary>
    ///     Whether the task is marked as completed (where 1 is true and 0 is false).
    /// </summary>
    [JsonProperty]
    public Int32 @checked { get; set; }

    /// <summary>
    ///     Whether the task is marked as deleted (where 1 is true and 0 is false).
    /// </summary>
    [JsonProperty]
    public Int32 is_deleted { get; set; }

    public Boolean IsToBeDeleted => labels != null && labels.Contains(Label.SpecialId_TaskToRemove);

    /// <summary>
    ///     Save copies of values that user can modify.
    ///     Original values are used do determine whether user changed something in reviewed task and whether API update call is necessary.
    /// </summary>
    public void SaveOriginalValues()
    {
        originalLabels = labels;
        originalPriority = priority;
        originalContent = content;
    }

    public Boolean ItemWasChangedByUser
    {
        get
        {
            Debug.Assert(originalPriority >= 1);
            Debug.Assert(originalPriority <= 4);
            Debug.Assert(priority >= 1);
            Debug.Assert(priority <= 4);
            Debug.Assert(originalTime >= 0);
            Debug.Assert(time >= 0);

            bool labelsDiffer = LabelsDiffer;
            bool priorityDiffers = priority != originalPriority;
            bool timeDiffers = time != originalTime;
            bool contentDiffers = content != originalContent;

            return labelsDiffer || priorityDiffers || timeDiffers || contentDiffers;
        }

    }
    private Boolean LabelsDiffer
    {
        get
        {
            // json deserialization returns null for empty arrays, so here's a conversion for an empty list
            if (labels == null)
            {
                labels = new List<Int64>();
            }
            if (originalLabels == null)
            {
                originalLabels = new List<Int64>();
            }

            HashSet<long>? set1unique = new HashSet<Int64>(labels);
            HashSet<long>? set2unique = new HashSet<Int64>(originalLabels);

            return !set1unique.SetEquals(set2unique);
        }
    }


    public void SetOriginalDurationInMinutes(Int32 durationTotalMinutes)
    {
        time = durationTotalMinutes;
        originalTime = durationTotalMinutes;
        timeEstimateWasAlreadyDefinedOnTheServerSide = true;
    }
}
