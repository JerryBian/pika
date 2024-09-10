using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Pika.Lib.Model;

public class PikaExport
{
    [JsonPropertyOrder(1)]
    [JsonPropertyName("tasks")]
    public List<PikaTask> Tasks { get; set; }

    [JsonPropertyOrder(2)]
    [JsonPropertyName("setting")]
    public PikaSetting Setting { get; set; }

    [JsonPropertyOrder(3)]
    [JsonPropertyName("apps")]
    public List<PikaApp> Apps { get; set; }

}