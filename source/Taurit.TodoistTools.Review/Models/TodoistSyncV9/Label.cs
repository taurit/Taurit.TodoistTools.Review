using Newtonsoft.Json;
using Taurit.TodoistTools.Review.Services;

// ReSharper disable InconsistentNaming - this class is returned to the client-side JS and data might be used there

namespace Taurit.TodoistTools.Review.Models.TodoistSyncV9;

/// <summary>
///     This class is returned to client-side code. Be cautious with name changes.
/// </summary>
public class Label
{
    internal const long SpecialId_TaskToRemove = -1;

    private const string GlyphiconDefaultClass = "glyphicon-paperclip";

    [Obsolete("Should only be used for deserialization")]
#pragma warning disable CS8618
    public Label()
#pragma warning restore CS8618
    {
    }

    public Label(long id, int isDeleted, string name)
    {
        this.id = id;
        is_deleted = isDeleted;
        this.name = name;
    }

    [JsonProperty]
    public long id { get; set; }

    [JsonProperty]
    public int is_deleted { get; set; }

    [JsonProperty]
    public string name { get; set; }

    [JsonProperty]
    public int item_order { get; set; }

    /// <summary>
    ///     Bootstrap glyphicon class for this label
    /// </summary>
    [JsonProperty]
    public string glyphicon
    {
        get
        {
            Dictionary<string, string> glyphiconDict = GlyphiconHelper.GetDictionary();

            string glyphiconClass = glyphiconDict.ContainsKey(name) ? glyphiconDict[name] : GlyphiconDefaultClass;
            return glyphiconClass;
        }
    }

    public static IReadOnlyList<Label> SpecialLabels { get; } = new List<Label>
        {
            new Label(SpecialId_TaskToRemove, 0, "eliminate") // special label: eliminate tasks
        };
}
