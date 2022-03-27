using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pika.Lib.Model;

namespace Pika.Web.Models;

public class TaskRunOutputViewModel
{
    [JsonPropertyName("startedAt")] public string StartedAt { get; set; }

    [JsonPropertyName("completedAt")] public string CompletedAt { get; set; }

    [JsonPropertyName("elapsed")] public string Elapsed { get; set; }

    [JsonPropertyName("startedAtTooltip")] public string StartedAtTooltip { get; set; }

    [JsonPropertyName("completedAtTooltip")] public string CompletedAtTooltip { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("outputs")] public List<PikaTaskRunOutput> Outputs { get; set; } = new();

    [JsonPropertyName("lastPoint")] public long LastPoint { get; set; }
}