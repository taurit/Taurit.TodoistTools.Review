using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace TodoistReview.Models
{
    public class GlyphiconHelper
    {
        private static string configFilePath = HostingEnvironment.MapPath("~/App_Data/glyphicons.json");
        private static Dictionary<string, string> instance = null;
        

        public static Dictionary<string, string> GetDictionary()
        {
            if (instance == null)
            {
                string jsonFileContent = System.IO.File.ReadAllText(configFilePath);
                instance = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonFileContent);    
            }

            return instance;
        }

    }
}