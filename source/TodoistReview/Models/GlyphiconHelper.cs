using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
using Newtonsoft.Json;

namespace TodoistReview.Models
{
    public class GlyphiconHelper
    {
        private static readonly String configFilePath = HostingEnvironment.MapPath("~/App_Data/glyphicons.json");
        private static Dictionary<String, String> instance;


        public static Dictionary<String, String> GetDictionary()
        {
            if (instance == null)
            {
                String jsonFileContent = File.ReadAllText(configFilePath);
                instance = JsonConvert.DeserializeObject<Dictionary<String, String>>(jsonFileContent);
            }

            return instance;
        }
    }
}