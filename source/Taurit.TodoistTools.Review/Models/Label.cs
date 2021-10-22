using Newtonsoft.Json;

// ReSharper disable InconsistentNaming - this class is returned to the client-side JS and data might be used there

namespace Taurit.TodoistTools.Review.Models;

/// <summary>
///     This class is returned to client-side code. Be cautious with name changes.
/// </summary>
public class Label
{
    internal const Int64 SpecialId_TaskToRemove = -1;

    private const String GlyphiconDefaultClass = "glyphicon-paperclip";

    [Obsolete("Should only be used for deserialization")]
    public Label()
    {
    }

    public Label(Int64 id, Int32 isDeleted, String name)
    {
        this.id = id;
        is_deleted = isDeleted;
        this.name = name;
    }

    [JsonProperty]
    public Int64 id { get; set; }

    [JsonProperty]
    public Int32 is_deleted { get; set; }

    [JsonProperty]
    public String name { get; set; }

    [JsonProperty]
    public Int32 item_order { get; set; }

    /// <summary>
    ///     Bootstrap glyphicon class for this label
    /// </summary>
    [JsonProperty]
    public String glyphicon
    {
        get
        {
            Dictionary<String, String> glyphiconDict = GlyphiconHelper.GetDictionary();

            String glyphiconClass = glyphiconDict.ContainsKey(name) ? glyphiconDict[name] : GlyphiconDefaultClass;
            return glyphiconClass;
        }
    }

    public static IReadOnlyList<Label> SpecialLabels { get; } = new List<Label>
        {
            new Label(SpecialId_TaskToRemove, 0, "eliminate") // special label: eliminate tasks
        };
}
