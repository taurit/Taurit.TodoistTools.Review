using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models
{
    public class TodoTask
    {
        public long id { get; set; }
        public string content { get; set; }
        public List<long> labels { get; set; }

        /// <summary>
        /// A copy of "labels" which should not be changed on the client side. 
        /// It helps to determine if user changed label collection for the task, and whether 
        /// calling update on Todoist API is needed.
        /// </summary>
        public List<long> originalLabels { get; set; }

        public bool LabelsDiffer
        {
            get
            {
                // json deserialization returns null for empty arrays, so here's a conversion for an empty list
                if (labels == null) labels = new List<long>();
                if (originalLabels == null) originalLabels = new List<long>();

                var set1unique = new HashSet<long>(labels);
                var set2unique = new HashSet<long>(originalLabels);

                return !set1unique.SetEquals(set2unique);
            }
        }

        public int priority { get; set; }
        public long project_id { get; set; }
    }
}