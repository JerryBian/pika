using System.Text.Json.Serialization;

namespace Pika.Common.Model;

public class PikaExport
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("setting")]
    public PikaSetting Setting { get; set; }

    [JsonPropertyOrder(2)]
    [JsonPropertyName("scripts")]
    public List<PikaScript> Scripts { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("apps")]
    public List<PikaApp> Apps { get; set; }
}