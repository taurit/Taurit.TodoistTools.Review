using System;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global - this class is returned to the client-side JS and data might be used there
// ReSharper disable InconsistentNaming - this class is returned to the client-side JS and data might be used there

namespace TodoistReview.Models
{
    /// <summary>
    ///     This class is returned to client-side code. Be cautious with name changes.
    /// </summary>
    public class Label
    {
        private const String GlyphiconDefaultClass = "glyphicon-paperclip";
        public Int64 id { get; }
        public Int32 is_deleted { get;  }
        public String name { get; }
        public Int32 item_order { get; }

        public Label(Int64 id, Int32 isDeleted, String name)
        {
            this.id = id;
            is_deleted = isDeleted;
            this.name = name;
        }

        /// <summary>
        ///     Bootstrap glyphicon class for this label
        /// </summary>
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