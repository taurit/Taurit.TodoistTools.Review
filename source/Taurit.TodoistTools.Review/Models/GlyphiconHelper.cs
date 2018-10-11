using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Taurit.TodoistTools.Review.Models
{
    public class GlyphiconHelper
    {
        private static Dictionary<String, String> _instance;
        
        public static Dictionary<String, String> GetDictionary()
        {
            if (_instance == null)
            {
                String jsonFileContent =
                    "{\r\n  \"agc\": \"glyphicon-hdd\",\r\n  \"home\": \"glyphicon-home\",\r\n  \"itm\": \"glyphicon-cog\",\r\n  \"ma\": \"glyphicon-cog\",\r\n  \"bs\": \"glyphicon-cog\",\r\n  \"kmd\": \"glyphicon-cog\",\r\n  \"laptop\": \"glyphicon-floppy-disk\",\r\n  \"market\": \"glyphicon-shopping-cart\",\r\n  \"marta\": \"glyphicon-comment\",\r\n  \"mobile\": \"glyphicon-phone\",\r\n  \"sanok\": \"glyphicon-tree-deciduous\",\r\n  \"warsaw\": \"glyphicon-envelope\",\r\n  \"eliminate\": \"glyphicon-remove\",\r\n  \"nawyk\": \"glyphicon-off\"\r\n}";
                _instance = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonFileContent);
            }

            return _instance;
        }
    }
}