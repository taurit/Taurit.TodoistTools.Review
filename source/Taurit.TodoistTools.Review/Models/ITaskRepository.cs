using System;
using System.Collections.Generic;

namespace Taurit.TodoistTools.Review.Models;

internal interface ITaskRepository
{
    IList<Label> GetAllLabels();
    IList<TodoTask> GetAllTasks();
    String UpdateTasks(List<TodoTask> tasksToUpdate);
}
