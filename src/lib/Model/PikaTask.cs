using System;
using System.Text.Json.Serialization;

namespace Pika.Lib.Model;

public class PikaTask
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("desc")] public string Description { get; set; }

    [JsonPropertyName("script")] public string Script { get; set; }

    [JsonPropertyName("shellName")] public string ShellName { get; set; }

    [JsonPropertyName("shellOption")] public string ShellOption { get; set; }

    [JsonPropertyName("shellExt")] public string ShellExt { get; set; }

    [JsonPropertyName("createdAt")] public DateTime CreatedAt { get; set; }

    [JsonPropertyName("lastModifiedAt")] public DateTime LastModifiedAt { get; set; }
}