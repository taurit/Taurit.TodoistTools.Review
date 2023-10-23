using NaturalLanguageTimespanParser;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Taurit.TodoistTools.Review.Models;
using Taurit.TodoistTools.Review.Models.TodoistSyncV9;
using Label = Taurit.TodoistTools.Review.Models.Label;
using TodoistTask = Taurit.TodoistTools.Review.Models.TodoistTask;

namespace Taurit.TodoistTools.Review.Services;

internal class TodoistSyncApiV9Client : ITodoistApiClient
{
    private const string ApiUrl = "https://api.todoist.com/sync/v9/sync";
    private readonly HttpClient _httpClient;
    private readonly TimespanParser _timespanParser;
    private readonly String? _todoistApiKey;

    public TodoistSyncApiV9Client(String? todoistApiKey, HttpClient httpClient,
        TimespanParser timespanParser)
    {
        _todoistApiKey = todoistApiKey;
        _httpClient = httpClient;
        _timespanParser = timespanParser;
    }

    public async Task<List<Label>> GetAllLabels()
    {
        // rest api is a bit more convenient and unlikely to change to the degree where I can't just get list of label names
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.todoist.com/rest/v2/labels");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _todoistApiKey);
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var labels = await response.Content.ReadFromJsonAsync<List<Models.TodoistSyncV9.Label>>();
        if (labels is null)
        {
            throw new InvalidOperationException(
                $"A request to get all labels failed: the status was {response.StatusCode}, but label list is null");
        }

        return labels.Select(x => new Label(x.Name)).ToList();
    }

    public async Task<List<TodoistTask>> GetAllTasks()
    {
        // rest api is a bit more convenient and unlikely to change to the degree where I can't just get list of label names
        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _todoistApiKey);
        var parameters = new Dictionary<string, string>
        {
            { "seq_no", "0" },
            { "resource_types", "[\"items\"]" }
        };
        request.Content = new FormUrlEncodedContent(parameters);

        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var tasksResponse = await response.Content.ReadFromJsonAsync<GetTodoistTasksResponse>();
        var taskModels = new List<TodoistTask>();

        foreach (Models.TodoistSyncV9.TodoistTask task in tasksResponse!.Items)
        {
            var estimateMinutes = 0;
            TimespanParseResult parsedDuration = _timespanParser.Parse(task.Content);
            if (parsedDuration.Success)
            {
                estimateMinutes = (Int32)parsedDuration.Duration.TotalMinutes;
            }

            List<Label> labels = task.Labels.Select(x => new Label(x)).ToList();
            var taskModel = new TodoistTask(task.Id, task.Content, task.Description, task.Priority, labels,
                estimateMinutes);
            taskModels.Add(taskModel);
        }

        return taskModels;
    }

    public async Task UpdateTasks(List<UpdatedTodoistTask> changedTasks)
    {
        if (changedTasks.Count == 0)
        {
            return;
        }

        String commands = BuildUpdateString(changedTasks);
        var parameters = new Dictionary<string, string>
        {
            { "commands", commands }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _todoistApiKey);
        request.Content = new FormUrlEncodedContent(parameters);
        HttpResponseMessage response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        Task<String> responseContentDebug = response.Content.ReadAsStringAsync();
    }

    private String BuildUpdateString(List<UpdatedTodoistTask> tasksToUpdate)
    {
        // build json command as string (a shortcut)
        var commandsString = new StringBuilder();
        commandsString.Append("[");
        for (var i = 0; i < tasksToUpdate.Count; i++)
        {
            string commandString = GetUpdateCommandString(tasksToUpdate[i]);
            commandsString.Append(commandString);

            if (i != tasksToUpdate.Count - 1)
            {
                commandsString.Append(",");
            }
        }

        commandsString.Append("]");
        return commandsString.ToString();
    }

    private string GetUpdateCommandString(UpdatedTodoistTask task)
    {
        string commandString;
        var commandId = Guid.NewGuid();

        if (task.Labels.Contains("eliminate"))
        {
            // as in documentation, https://developer.todoist.com/sync/v9/#delete-item
            commandString =
                $"{{\"type\": \"item_delete\", \"uuid\": \"{commandId}\", \"args\": {{\"id\": \"{task.OriginalTask.Id}\" }}}}";
        }
        else
        {
            // typical use case: update labels
            var commandObject = new
            {
                type = "item_update",
                uuid = $"{commandId}",
                args = new
                {
                    id = task.OriginalTask.Id,
                    priority = task.Priority,
                    labels = task.Labels.Where(x => x != "eliminate").ToList(),
                    content = task.ContentWithTimeMetadata,
                    duration = new
                    {
                        amount = task.EstimatedTimeMinutes,
                        unit = "minute"
                    }
                }
            };
            commandString = JsonSerializer.Serialize(commandObject);
        }

        return commandString;
    }
}
