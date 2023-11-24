using Pika.Lib.Model;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pika.Web.Models;

public class TaskRunOutputViewModel
{
    [JsonPropertyName("startedAt")] public string StartedAt { get; set; }

    [JsonPropertyName("completedAt")] public string CompletedAt { get; set; }

    [JsonPropertyName("elapsed")] public string Elapsed { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("outputs")] public List<PikaTaskRunOutput> Outputs { get; set; } = [];

    [JsonPropertyName("lastPoint")] public string LastPoint { get; set; }
}