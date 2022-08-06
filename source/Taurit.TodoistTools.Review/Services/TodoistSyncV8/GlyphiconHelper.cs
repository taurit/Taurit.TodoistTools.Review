using Newtonsoft.Json;

namespace Taurit.TodoistTools.Review.Services.TodoistSyncV8;

public static class GlyphiconHelper
{
    private static readonly Dictionary<string, string> Instance = JsonConvert.DeserializeObject<Dictionary<string, string>>(
        "{\r\n  \r\n  \"home\": \"glyphicon-home\",\r\n  \"laptop\": \"glyphicon-floppy-disk\",\r\n  \"market\": \"glyphicon-shopping-cart\",\r\n  \"work\": \"glyphicon-comment\",\r\n  \"mobile\": \"glyphicon-phone\",\r\n  \"sanok\": \"glyphicon-tree-deciduous\",\r\n  \"warsaw\": \"glyphicon-envelope\",\r\n  \"eliminate\": \"glyphicon-remove\",\r\n  \"nawyk\": \"glyphicon-off\"\r\n}"
        )!;

    public static Dictionary<string, string> GetDictionary()
    {
        return Instance;
    }
}
