using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using TodoistReview.Models.TodoistApiModels;

namespace TodoistReview.Models
{
    public class TodoistTaskRepository : ITaskRepository
    {
        private string _authToken;
        private const string apiUrl = "https://todoist.com/API/v6/";

        public TodoistTaskRepository(string authToken)
        {
            this._authToken = authToken;
        }

        public IList<Label> GetAllLabels()
        {
            var client = new RestClient(apiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);
            request.AddParameter("seq_no", "0");
            request.AddParameter("resource_types", "[\"labels\"]");

            IRestResponse<TodoistLabelsResponse> response = client.Execute<TodoistLabelsResponse>(request);
            var content = response.Content;

            return response.Data.Labels;
        }

        public IList<TodoTask> GetAllTasks()
        {
            var client = new RestClient(apiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);

            // Sequence number, used to allow client to perform incremental sync. Pass 0 to retrieve all active resource data. 
            request.AddParameter("seq_no", "0");
            request.AddParameter("resource_types", "[\"items\"]");
            
            IRestResponse<TodoistTasksResponse> response = client.Execute<TodoistTasksResponse>(request);
            var content = response.Content;

            return response.Data.Items;
        }

        public string UpdateTasks(List<TodoTask> tasksToUpdate)
        {
            if (tasksToUpdate.Count == 0) return "Empty list of tasks";
            if (tasksToUpdate.Any(task => task.labels == null)) return "List of tasks contains at least one invalid item";

            var client = new RestClient(apiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);

            /// build json command as string (a shortcut)
            StringBuilder commandsString = new StringBuilder();
            commandsString.Append("[");
            for (int i = 0; i < tasksToUpdate.Count; i++)
            {
                string commandString = this.GetUpdateCommandString(tasksToUpdate[i]);
                commandsString.Append(commandString);

                if (i != tasksToUpdate.Count - 1)
                    commandsString.Append(",");
            }

            commandsString.Append("]");


            request.AddParameter("commands", commandsString.ToString());

            IRestResponse<TodoistTasksResponse> response = client.Execute<TodoistTasksResponse>(request);
            string apiResponse = response.Content;
            return apiResponse;
        }

        private string GetUpdateCommandString(TodoTask task)
        {
            var commandId = Guid.NewGuid();
            var labelsArrayString = "[" + String.Join(",", task.labels) + "]"; // json array with int64 ids

            var commandString = "{\"type\": \"item_update\", \"uuid\": \"" + commandId + "\", \"args\": {\"id\": " + task.id + ", \"labels\": " + labelsArrayString + "}}";

            return commandString;
        }
    }
}