namespace Pika.Models
{
    public class PikaDriveIndexViewModel
    {
        public string DriveId { get; set; }

        public string DrivePath { get; set; }

        public string DriveType { get; set; }

        public string PowerStatusSpanId => $"{DriveId.Replace(" ", "").Replace("-", "")}_PowerStatus";

        public string MountPoints { get; set; }

        public string Size { get; set; }

        public string DeviceIcon => DriveType == "HDD" ? "bi-hdd-fill" : "bi-device-ssd";
    }
}
