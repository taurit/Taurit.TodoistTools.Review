using System;
using System.Collections.Generic;

namespace TodoistReview.Models
{
    public class Label
    {
        private const String glyphiconDefaultClass = "glyphicon-paperclip";
        public Int64 id { get; set; }
        public Int32 is_deleted { get; set; }
        public String name { get; set; }
        public Int32 item_order { get; set; }

        /// <summary>
        ///     Bootstrap glyphicon class for this label
        /// </summary>
        public String glyphicon
        {
            get
            {
                Dictionary<String, String> glyphiconDict = GlyphiconHelper.GetDictionary();

                String glyphiconClass = glyphiconDict.ContainsKey(name) ? glyphiconDict[name] : glyphiconDefaultClass;
                return glyphiconClass;
            }
        }
    }
}