using System.Net;
using System.Net.Http.Headers;
using NaturalLanguageTimespanParser;
using Taurit.TodoistTools.Review.Models;
using Taurit.TodoistTools.Review.Models.TodoistSyncV9;
using Label = Taurit.TodoistTools.Review.Models.Label;
using TodoistTask = Taurit.TodoistTools.Review.Models.TodoistTask;

namespace Taurit.TodoistTools.Review.Services;

internal class TodoistSyncApiV9Client : ITodoistApiClient
{
    private const string ApiUrl = "https://api.todoist.com/sync/v9/sync";
    private readonly HttpClient _httpClient;
    private readonly MultiCultureTimespanParser _timespanParser;
    private readonly String? _todoistApiKey;

    public TodoistSyncApiV9Client(String? todoistApiKey, HttpClient httpClient, MultiCultureTimespanParser timespanParser)
    {
        _todoistApiKey = todoistApiKey;
        _httpClient = httpClient;
        _timespanParser = timespanParser;
    }

    public async Task<List<Label>> GetAllLabels()
    {
        // rest api is a bit more convenient and unlikely to change to the degree where I can't just get list of label names
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.todoist.com/rest/v1/labels");
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

        foreach (var task in tasksResponse!.Items)
        {
            var estimateMinutes = 0;
            TimespanParseResult parsedDuration = _timespanParser.Parse(task.Content);
            if (parsedDuration.Success)
            {
                estimateMinutes = (Int32)parsedDuration.Duration.TotalMinutes;
            }
            var labels = task.Labels.Select(x => new Label(x)).ToList();
            var taskModel = new TodoistTask(task.Id, task.Content, task.Description, task.Priority, labels,
                estimateMinutes);
            taskModels.Add(taskModel);
        }
        
        return taskModels;
    }

    public Task UpdateTasks(List<UpdatedTodoistTask> changedTasks)
    {
        throw new NotImplementedException();
    }
}
