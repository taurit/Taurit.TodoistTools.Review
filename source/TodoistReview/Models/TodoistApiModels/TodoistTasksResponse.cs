using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models.TodoistApiModels
{
    public class TodoistTasksResponse
    {
        public long seq_no_global { get; set; }
        public List<TodoTask> Items { get; set; }

        public long UserId { get; set; }
    }
}