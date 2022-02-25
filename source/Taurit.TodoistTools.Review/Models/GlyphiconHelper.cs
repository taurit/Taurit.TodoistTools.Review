using Newtonsoft.Json;

namespace Taurit.TodoistTools.Review.Models;

public static class GlyphiconHelper
{
    private static readonly Dictionary<String, String> Instance = JsonConvert.DeserializeObject<Dictionary<String, String>>(
        "{\r\n  \r\n  \"home\": \"glyphicon-home\",\r\n  \"laptop\": \"glyphicon-floppy-disk\",\r\n  \"market\": \"glyphicon-shopping-cart\",\r\n  \"work\": \"glyphicon-comment\",\r\n  \"mobile\": \"glyphicon-phone\",\r\n  \"sanok\": \"glyphicon-tree-deciduous\",\r\n  \"warsaw\": \"glyphicon-envelope\",\r\n  \"eliminate\": \"glyphicon-remove\",\r\n  \"nawyk\": \"glyphicon-off\"\r\n}"
        )!;

    public static Dictionary<String, String> GetDictionary()
    {
        return Instance;
    }
}
