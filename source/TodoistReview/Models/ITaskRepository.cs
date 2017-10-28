using System;
using System.Collections.Generic;

namespace TodoistReview.Models
{
    internal interface ITaskRepository
    {
        IList<Label> GetAllLabels();
        IList<TodoTask> GetAllTasks();
        String UpdateTasks(List<TodoTask> tasksToUpdate);
    }
}