using System;
using System.Text.Json.Serialization;

namespace Pika.Common.Model;

public class PikaTaskRunOutput
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("runId")] public long TaskRunId { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("isError")] public bool IsError { get; set; }

    [JsonPropertyName("createdAt")] public long CreatedAt { get; set; }

    [JsonPropertyName("createdAtString")] public string CreatedAtString => new DateTime(CreatedAt).ToString("O");
}