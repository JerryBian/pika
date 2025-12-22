using System.Text.Json.Serialization;

namespace Pika.Common.Drive
{
    public class PikaDriveLsblkTable
    {
        public string DriveId { get; set; }

        public string JsonOutput { get; set; }

        public long CreatedAt { get; set; }
    }

    public class PikaDriveLsblk
    {
        [JsonPropertyName("blockdevices")]
        public List<PikaDriveLsblkBlockDevice> BlockDevices { get; set; }
    }

    public class PikaDriveLsblkBlockDevice
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("model")]
        public string Model { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("path")]
        public string Path { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("size")]
        public string Size { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("fssize")]
        public string FsSize { get; set; }

        [JsonPropertyName("fstype")]
        public string FsType { get; set; }

        [JsonPropertyName("fsused")]
        public string FsUsed { get; set; }

        [JsonPropertyName("serial")]
        public string Serial { get; set; }

        [JsonPropertyName("fsuse%")]
        public string FsUsePct { get; set; }

        [JsonPropertyName("mountpoint")]
        public string MountPoint { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("tran")]
        public string Tran { get; set; }

        [JsonPropertyName("rota")]
        public bool Rota { get; set; }

        [JsonPropertyName("children")]
        public List<PikaDriveLsblkBlockDevice> Children { get; set; }
    }
}
