using System;
using System.Text.Json.Serialization;

namespace Pika.Lib.Model;

public class PikaTaskRunOutput
{
    public long Id { get; set; }

    public long TaskRunId { get; set; }

    [JsonPropertyName("message")] public string Message { get; set; }

    [JsonPropertyName("isError")] public bool IsError { get; set; }

    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }
}