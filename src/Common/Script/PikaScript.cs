using System.Text.Json.Serialization;

namespace Pika.Common.Script
{
    public class PikaScript
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("desc")] public string Description { get; set; }

        [JsonPropertyName("script")] public string Script { get; set; }

        [JsonPropertyName("shellName")] public string ShellName { get; set; }

        [JsonPropertyName("shellOption")] public string ShellOption { get; set; }

        [JsonPropertyName("shellExt")] public string ShellExt { get; set; }

        [JsonPropertyName("createdAt")] public long CreatedAt { get; set; }

        [JsonPropertyName("lastModifiedAt")] public long LastModifiedAt { get; set; }

        [JsonPropertyName("isTemp")] public bool IsTemp { get; set; }

        public int RunCount { get; set; }
    }
}
