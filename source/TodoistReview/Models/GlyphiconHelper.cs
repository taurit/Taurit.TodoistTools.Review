using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace TodoistReview.Models
{
    public class GlyphiconHelper
    {
        private static readonly String ConfigFilePath = HostingEnvironment.MapPath("~/App_Data/glyphicons.json");
        private static Dictionary<String, String> _instance;
        
        public static Dictionary<String, String> GetDictionary()
        {
            if (_instance == null)
            {
                String jsonFileContent = File.ReadAllText(ConfigFilePath);
                _instance = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonFileContent);
            }

            return _instance;
        }
    }
}