using System;
using System.Collections.Generic;

namespace TodoistReview.Models
{
    /// <summary>
    ///     Field names as in: https://developer.todoist.com/#items
    /// </summary>
    public class TodoTask
    {
        public Int64 id { get; set; }
        public String content { get; set; }
        public List<Int64> labels { get; set; }

        /// <summary>
        ///     A copy of "labels" which should not be changed on the client side.
        ///     It helps to determine if user changed label collection for the task, and whether
        ///     calling update on Todoist API is needed.
        /// </summary>
        public List<Int64> originalLabels { get; set; }

        public Boolean LabelsDiffer
        {
            get
            {
                // json deserialization returns null for empty arrays, so here's a conversion for an empty list
                if (labels == null)
                {
                    labels = new List<Int64>();
                }
                if (originalLabels == null)
                {
                    originalLabels = new List<Int64>();
                }

                var set1unique = new HashSet<Int64>(labels);
                var set2unique = new HashSet<Int64>(originalLabels);

                return !set1unique.SetEquals(set2unique);
            }
        }

        public Int32 priority { get; set; }
        public Int64 project_id { get; set; }

        /// <summary>
        ///     Whether the task is marked as completed (where 1 is true and 0 is false).
        /// </summary>
        public Int32 @checked { get; set; }

        /// <summary>
        ///     Whether the task is marked as deleted (where 1 is true and 0 is false).
        /// </summary>
        public Int32 is_deleted { get; set; }
    }
}