using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text;
using Taurit.TodoistTools.Review.Models.TodoistApiModels;

namespace Taurit.TodoistTools.Review.Models;

public class TodoistTaskRepository : ITaskRepository
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded")]
    private const String ApiUrl = "https://api.todoist.com/sync/v8/";

    private readonly String _authToken;

    public TodoistTaskRepository(String authToken)
    {
        _authToken = authToken;

        // in .NET 4.7 defaults changed and RestSharp cannot establish secure SSL/TLS connecion without explicitly stating that the server needs newer TLS version
        // https://stackoverflow.com/questions/44751179/tls-1-2-not-negotiated-in-net-4-7-without-explicit-servicepointmanager-security
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    public async Task<IList<Label>> GetAllLabels()
    {
        RestClient client = new RestClient(ApiUrl);

        RestRequest request = new RestRequest("sync", Method.Post);
        request.AddParameter("token", _authToken);
        request.AddParameter("seq_no", "0");
        request.AddParameter("resource_types", "[\"labels\"]");

        RestResponse<TodoistLabelsResponse> response = await client.ExecuteAsync<TodoistLabelsResponse>(request);

        if (!response.IsSuccessful)
        {
            throw new InvalidOperationException(
                $"A request to get all labels failed: the status was {response.StatusCode}, {response.ErrorMessage}");
        }

        if (response.Data is null)
        {
            throw new InvalidOperationException(
                $"A request to get all labels failed: the status was {response.StatusCode}, but Data is empty");
        }

        return response.Data.Labels ?? new List<Label>(0);
    }

    public async Task<IList<TodoTask>> GetAllTasks()
    {
        RestClient client = new RestClient(ApiUrl);

        RestRequest request = new RestRequest("sync", Method.Post);
        request.AddParameter("token", _authToken);

        // Sequence number, used to allow client to perform incremental sync. Pass 0 to retrieve all active resource data. 
        request.AddParameter("seq_no", "0");
        request.AddParameter("resource_types", "[\"items\"]");

        RestResponse<TodoistTasksResponse> response = await client.ExecuteAsync<TodoistTasksResponse>(request);

        if (!response.IsSuccessful)
        {
            throw new InvalidOperationException(
                $"A request to get all tasks failed: the status was {response.StatusCode}, {response.ErrorMessage}");
        }

        if (response.Data is null)
        {
            throw new InvalidOperationException(
                $"A request to get all tasks failed: the status was {response.StatusCode}, but Data is empty");
        }

        return response.Data.Items ?? new List<TodoTask>(0);
    }

    public async Task<String> UpdateTasks(List<TodoTask> tasksToUpdate)
    {
        if (tasksToUpdate.Count == 0)
        {
            return "Empty list of tasks";
        }
        if (tasksToUpdate.Any(task => task.labels == null || task.time < 0 || task.content == null))
        {
            return "List of tasks contains at least one invalid item";
        }

        RestClient client = new RestClient(ApiUrl);

        RestRequest request = new RestRequest("sync", Method.Post);
        request.AddParameter("token", _authToken);

        // build json command as string (a shortcut)
        StringBuilder? commandsString = new StringBuilder();
        commandsString.Append("[");
        for (int i = 0; i < tasksToUpdate.Count; i++)
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

        RestResponse<TodoistTasksResponse> response = await client.ExecuteAsync<TodoistTasksResponse>(request);
        String apiResponse = response.Content ?? "null";
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
            long[]? labelsExcludingSpecial =
                task.labels?.Where(x => !specialLabelsIds.Contains(x)).ToArray();

            var commandObject = new
            {
                type = "item_update",
                uuid = $"{commandId}",
                args = new
                {
                    task.id,
                    task.priority,
                    labels = labelsExcludingSpecial,
                    content = task.contentWithTime
                }
            };
            commandString = JsonConvert.SerializeObject(commandObject);
        }

        return commandString;
    }
}
