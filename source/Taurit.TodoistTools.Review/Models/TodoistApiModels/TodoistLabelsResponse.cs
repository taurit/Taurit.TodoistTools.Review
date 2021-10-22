using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming

namespace Taurit.TodoistTools.Review.Models.TodoistApiModels;

internal class TodoistLabelsResponse
{
    [JsonProperty]
    public List<Label> Labels { get; set; }

    [JsonProperty]
    public Int64 seq_no_global { get; set; }

    [JsonProperty]
    public Int64 UserId { get; set; }
}
