using Pika.Common.Drive;

namespace Pika.Models
{
    public class PikaDriveDetailViewModel
    {
        public string DeviceId { get; set; }

        public string DeviceIcon => DeviceType == "HDD" ? "bi-hdd-fill" : "bi-device-ssd";

        public string DeviceName { get; set; }

        public string DevicePath { get; set; }

        public string DeviceType { get; set; }

        public string Size { get; set; }

        public string Model { get; set; }

        public string SerialNumber { get; set; }

        public string PowerState { get; set; } = "-";

        public string PowerStateClass { get; set; } = "text-info";

        public List<PikaDriveSmartctlTable> SmartctlTable { get; set; } = new();

        public List<PikaDriveDetailViewModelPartition> Partitions { get; } = new();
    }

    public class PikaDriveDetailViewModelPartition
    {
        public string PartitionName { get; set; }

        public string PartitionPath { get; set; }

        public string MountPoint { get; set; }

        public string FileSystemType { get; set; }

        public string Label { get; set; }

        public string Uuid { get; set; }

        public string Capacity { get; set; } = "-";
    }
}
