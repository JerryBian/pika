using Pika.Common.Model;
using System.Text.Json.Serialization;

namespace Pika.Models
{
    public class PikaScriptRunOutputViewModel
    {
        [JsonPropertyName("startedAt")] public string StartedAt { get; set; }

        [JsonPropertyName("completedAt")] public string CompletedAt { get; set; }

        [JsonPropertyName("elapsed")] public string Elapsed { get; set; }

        [JsonPropertyName("status")] public string Status { get; set; }

        [JsonPropertyName("outputs")] public List<PikaScriptRunOutput> Outputs { get; set; } = [];

        [JsonPropertyName("lastPoint")] public string LastPoint { get; set; }
    }
}
