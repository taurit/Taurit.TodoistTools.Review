using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models.TodoistApiModels
{
    internal class TodoistLabelsResponse
    {
        public long seq_no_global { get; set; }
        public List<Label> Labels { get; set; }

        public long UserId { get; set; }
    }
}