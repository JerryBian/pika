using System.Text.Json.Serialization;

namespace Pika.Lib.Model;

public class PikaSetting
{
    [JsonPropertyName("defaultShellName")] public string DefaultShellName { get; set; }

    [JsonPropertyName("defaultShellOption")]
    public string DefaultShellOption { get; set; }

    [JsonPropertyName("defaultShellExt")] public string DefaultShellExt { get; set; }

    [JsonPropertyName("itemsPerPage")] public int ItemsPerPage { get; set; }
}