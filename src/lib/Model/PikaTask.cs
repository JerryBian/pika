using System;
using System.Collections.Generic;
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

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }

    public List<PikaTaskRun> Runs { get; set; } = new();
}