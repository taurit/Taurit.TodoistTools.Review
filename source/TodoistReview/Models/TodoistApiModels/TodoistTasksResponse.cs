using System;
using System.Collections.Generic;

namespace TodoistReview.Models.TodoistApiModels
{
    public class TodoistTasksResponse
    {
        public Int64 seq_no_global { get; set; }
        public List<TodoTask> Items { get; set; }

        public Int64 UserId { get; set; }
    }
}