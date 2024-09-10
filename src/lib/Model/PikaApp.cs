using System.Text.Json.Serialization;

namespace Pika.Lib.Model
{
    public class PikaApp
    {
        [JsonPropertyName("id")] public long Id { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("start_script")] public string StartScript { get; set; }

        [JsonPropertyName("start_script_path")] public string StartScriptPath { get; set; }

        [JsonPropertyName("stop_script")] public string StopScript { get; set; }

        [JsonPropertyName("stop_script_path")] public string StopScriptPath { get; set; }

        [JsonPropertyName("shell_name")] public string ShellName { get; set; }

        [JsonPropertyName("shell_option")] public string ShellOption { get; set; }

        [JsonPropertyName("shell_ext")] public string ShellExt { get; set; }

        [JsonPropertyName("createdAt")] public long CreatedAt { get; set; }

        [JsonPropertyName("lastModifiedAt")] public long LastModifiedAt { get; set; }
    }
}
