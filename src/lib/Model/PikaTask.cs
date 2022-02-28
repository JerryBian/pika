using System;
using System.Text.Json.Serialization;

namespace Pika.Lib.Model;

public class PikaTask
{
    [JsonPropertyName("id")] public long Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; }

    [JsonPropertyName("desc")] public string Description { get; set; }

    [JsonPropertyName("script")] public string Script { get; set; }

    [JsonPropertyName("isFavorite")] public bool IsFavorite { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime LastModifiedAt { get; set; }
}