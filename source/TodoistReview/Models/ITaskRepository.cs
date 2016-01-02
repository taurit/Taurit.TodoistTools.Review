using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TodoistReview.Models
{
    interface ITaskRepository
    {
        IList<Label> GetAllLabels();
        IList<TodoTask> GetAllTasks();
        string UpdateTasks(List<TodoTask> tasksToUpdate);
    }
}