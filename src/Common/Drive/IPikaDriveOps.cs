
namespace Pika.Common.Drive
{
    public interface IPikaDriveOps
    {
        Task<List<PikaDriveTable>> GetActivePikaDrivesAsync();
        Task<PikaDrivePowerStatus> GetDevicePowerStatusAsync(string devicePath);
        List<DriveInfo> GetDriveInfo();
        Task<List<PikaDriveSmartctlTable>> GetRecentPikaDriveSmartctlAsync(string driveId);
        Task<PikaDriveLsblk> RunPikaDriveLsblkAsync();
        Task<PikaDriveSmartctlTable> RunPikaDriveSmartctlAsync(string drivePath);
        Task UpdatePikaDriveAsync();
    }
}