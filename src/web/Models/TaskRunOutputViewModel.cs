using System.Collections.Generic;
using System.Text.Json.Serialization;
using Pika.Lib.Model;

namespace Pika.Web.Models;

public class TaskRunOutputViewModel
{
    [JsonPropertyName("taskId")] public long TaskId { get; set; }

    [JsonPropertyName("taskName")] public string TaskName { get; set; }

    [JsonPropertyName("runId")] public long RunId { get; set; }

    [JsonPropertyName("runStartAt")] public string RunStartAt { get; set; }

    [JsonPropertyName("runEndAt")] public string RunEndAt { get; set; }

    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("outputs")] public List<PikaTaskRunOutput> Outputs { get; set; }

    [JsonPropertyName("maxTimestamp")] public long MaxTimestamp { get; set; }

    [JsonPropertyName("script")] public string Script { get; set; }

    [JsonPropertyName("foo")] public string Foo { get; set; }
}