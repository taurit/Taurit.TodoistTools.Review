using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models
{
    public class Label
    {
        public long id { get; set; }
        public int is_deleted { get; set; }
        public string name { get; set; }
        public int item_order { get; set; }

        private const string glyphiconDefaultClass = "glyphicon-paperclip";
        /// <summary>
        /// Bootstrap glyphicon class for this label
        /// </summary>
        public string glyphicon
        {
            get
            {
                Dictionary<string, string> glyphiconDict = GlyphiconHelper.GetDictionary();
                
                string glyphiconClass = glyphiconDict.ContainsKey(name) ? glyphiconDict[name] : glyphiconDefaultClass;
                return glyphiconClass; 
            }
        }
    }
}