using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using Taurit.TodoistTools.Review.Models.TodoistApiModels;

namespace Taurit.TodoistTools.Review.Models
{
    public class TodoistTaskRepository : ITaskRepository
    {
        private const String ApiUrl = "https://api.todoist.com/sync/v8/";
        private readonly String _authToken;

        public TodoistTaskRepository(String authToken)
        {
            _authToken = authToken;

            // in .NET 4.7 defaults changed and RestSharp cannot establish secure SSL/TLS connecion without explicitly stating that the server needs newer TLS version
            // https://stackoverflow.com/questions/44751179/tls-1-2-not-negotiated-in-net-4-7-without-explicit-servicepointmanager-security
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public IList<Label> GetAllLabels()
        {
            var client = new RestClient(ApiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);
            request.AddParameter("seq_no", "0");
            request.AddParameter("resource_types", "[\"labels\"]");

            IRestResponse<TodoistLabelsResponse> response = client.Execute<TodoistLabelsResponse>(request);

            return response.Data.Labels;
        }

        public IList<TodoTask> GetAllTasks()
        {
            var client = new RestClient(ApiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);

            // Sequence number, used to allow client to perform incremental sync. Pass 0 to retrieve all active resource data. 
            request.AddParameter("seq_no", "0");
            request.AddParameter("resource_types", "[\"items\"]");

            IRestResponse<TodoistTasksResponse> response = client.Execute<TodoistTasksResponse>(request);

            return response.Data.Items;
        }

        public String UpdateTasks(List<TodoTask> tasksToUpdate)
        {
            if (tasksToUpdate.Count == 0)
            {
                return "Empty list of tasks";
            }
            if (tasksToUpdate.Any(task => task.labels == null || task.time < 0 || task.content == null))
            {
                return "List of tasks contains at least one invalid item";
            }

            var client = new RestClient(ApiUrl);

            var request = new RestRequest("sync", Method.POST);
            request.AddParameter("token", _authToken);

            // build json command as string (a shortcut)
            var commandsString = new StringBuilder();
            commandsString.Append("[");
            for (var i = 0; i < tasksToUpdate.Count; i++)
            {
                String commandString = GetUpdateCommandString(tasksToUpdate[i]);
                commandsString.Append(commandString);

                if (i != tasksToUpdate.Count - 1)
                {
                    commandsString.Append(",");
                }
            }

            commandsString.Append("]");
            request.AddParameter("commands", commandsString.ToString());

            IRestResponse<TodoistTasksResponse> response = client.Execute<TodoistTasksResponse>(request);
            String apiResponse = response.Content;
            return apiResponse;
        }

        private String GetUpdateCommandString(TodoTask task)
        {
            String commandString;
            Guid commandId = Guid.NewGuid();

            if (task.IsToBeDeleted)
            {
                // as in documentation, https://developer.todoist.com/sync/v8/#delete-items
                commandString =
                    $"{{\"type\": \"item_delete\", \"uuid\": \"{commandId}\", \"args\": {{\"id\": {task.id} }}}}";
            }
            else
            {
                // typical use case: update labels
                List<Int64> specialLabelsIds = Label.SpecialLabels.Select(x => x.id).ToList();
                var labelsExcludingSpecial =
                    task.labels.Where(x => !specialLabelsIds.Contains(x)).ToArray();

                var commandObject = new
                {
                    type = "item_update",
                    uuid = $"{commandId}",
                    args = new {
                        id = task.id,
                        priority = task.priority,
                        labels = labelsExcludingSpecial,
                        content = task.contentWithTime
                    }
                };
                commandString = JsonConvert.SerializeObject(commandObject);
            }
            
            return commandString;
        }
    }
}