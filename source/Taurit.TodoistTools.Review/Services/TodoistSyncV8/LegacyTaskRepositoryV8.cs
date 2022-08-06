using System.Net;
using System.Text;
using Newtonsoft.Json;
using RestSharp;
using Taurit.TodoistTools.Review.Models.TodoistSyncV8;

namespace Taurit.TodoistTools.Review.Services.TodoistSyncV8;

[Obsolete("Todoist Sync API v8 won't work after 2022-11-01.")]
public class LegacyTaskRepositoryV8 : ILegacyTaskRepository
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1075:URIs should not be hardcoded")]
    private const string ApiUrl = "https://api.todoist.com/sync/v8/";

    private readonly string _authToken;

    public LegacyTaskRepositoryV8(string authToken)
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

    public async Task<string> UpdateTasks(List<TodoTask> tasksToUpdate)
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
            string commandString = GetUpdateCommandString(tasksToUpdate[i]);
            commandsString.Append(commandString);

            if (i != tasksToUpdate.Count - 1)
            {
                commandsString.Append(",");
            }
        }

        commandsString.Append("]");
        request.AddParameter("commands", commandsString.ToString());

        RestResponse<TodoistTasksResponse> response = await client.ExecuteAsync<TodoistTasksResponse>(request);
        string apiResponse = response.Content ?? "null";
        return apiResponse;
    }

    private string GetUpdateCommandString(TodoTask task)
    {
        string commandString;
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
            List<long> specialLabelsIds = Label.SpecialLabels.Select(x => x.id).ToList();
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
