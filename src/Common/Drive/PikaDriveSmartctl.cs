using System.Text.Json.Serialization;

namespace Pika.Common.Drive
{
    public class PikaDriveSmartctl
    {
        [JsonPropertyName("smartctl")]
        public PikaDriveSmartctlSmartctl Smartctl { get; set; }

        [JsonPropertyName("ata_smart_attributes")]
        public PikaDriveSmartctlAtaSmartAttributes AtaSmartAttributes { get; set; }

        [JsonPropertyName("smart_status")]
        public PikaDriveSmartctlSmartctlSmartStatus SmartStatus { get; set; }
    }

    public class PikaDriveSmartctlSmartctl
    {
        [JsonPropertyName("exit_status")]
        public int ExitStatus { get; set; }
    }

    public class PikaDriveSmartctlSmartctlSmartStatus
    {
        [JsonPropertyName("passed")]
        public bool Passed { get; set; }
    }

    public class PikaDriveSmartctlAtaSmartAttributes
    {
        [JsonPropertyName("table")]
        public List<PikaDriveSmartctlAtaSmartAttributesTable> Table { get; set; } = new();
    }

    public class PikaDriveSmartctlAtaSmartAttributesTable
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public int Value { get; set; }

        [JsonPropertyName("worst")]
        public object Worst { get; set; }

        [JsonPropertyName("thresh")]
        public object Thresh { get; set; }

        [JsonPropertyName("raw")]
        public PikaDriveSmartctlAtaSmartAttributesTableRaw Raw { get; set; }
    }

    public class PikaDriveSmartctlAtaSmartAttributesTableRaw
    {
        [JsonPropertyName("value")]
        public object Value { get; set; }

        [JsonPropertyName("string")]
        public string String { get; set; }
    }
}
