using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming - this class is returned to the client-side JS and data might be used there

namespace TodoistReview.Models
{
    /// <summary>
    ///     This class is returned to client-side code. Be cautious with name changes.
    /// </summary>
    public class Label
    {
        private const String GlyphiconDefaultClass = "glyphicon-paperclip";

        public Label(Int64 id, Int32 isDeleted, String name)
        {
            this.id = id;
            is_deleted = isDeleted;
            this.name = name;
        }

        [JsonProperty]
        public Int64 id { get; private set; }

        [JsonProperty]
        public Int32 is_deleted { get; private set; }

        [JsonProperty]
        public String name { get; private set; }

        [JsonProperty]
        public Int32 item_order { get; private set; }

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
    }
}