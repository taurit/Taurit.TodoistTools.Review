using System;
using System.Collections.Generic;

namespace TodoistReview.Models.TodoistApiModels
{
    internal class TodoistLabelsResponse
    {
        public Int64 seq_no_global { get; set; }
        public List<Label> Labels { get; set; }

        public Int64 UserId { get; set; }
    }
}