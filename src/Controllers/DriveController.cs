using Microsoft.AspNetCore.Mvc;
using Pika.Common.Drive;
using Pika.Common.Extension;
using Pika.Models;

namespace Pika.Controllers
{
    [Route("drive")]
    public class DriveController : Controller
    {
        private readonly IPikaDriveOps _pikaDriveOps;

        public DriveController(IPikaDriveOps pikaDriveOps)
        {
            _pikaDriveOps = pikaDriveOps;
        }

        public async Task<IActionResult> Index()
        {
            var model = new List<PikaDriveIndexViewModel>();
            var activeDrives = await _pikaDriveOps.GetActivePikaDrivesAsync();
            foreach (var drive in activeDrives)
            {
                var m = new PikaDriveIndexViewModel
                {
                    DrivePath = drive.Path,
                    DriveType = drive.Type,
                    Size = drive.Size,
                    DriveId = drive.Id,
                    MountPoints = string.Join(" & ", drive.Partitions.Select(p => p.MountPoint).Where(x => !string.IsNullOrWhiteSpace(x))),
                };

                model.Add(m);
            }

            return View(model);
        }

        [Route("{driveId}")]
        public async Task<IActionResult> Detail(string driveId)
        {
            var activeDrives = await _pikaDriveOps.GetActivePikaDrivesAsync();
            var drive = activeDrives.FirstOrDefault(d => d.Id == driveId);
            if (drive == null)
            {
                return NotFound();
            }

            var model = new PikaDriveDetailViewModel
            {
                DeviceId = driveId,
                DeviceName = drive.Name,
                DevicePath = drive.Path,
                DeviceType = drive.Type,
                Model = drive.Model,
                SerialNumber = drive.Serial,
                Size = drive.Size,
                SmartctlTable = await _pikaDriveOps.GetRecentPikaDriveSmartctlAsync(driveId),
            };
            var driveInfo = _pikaDriveOps.GetDriveInfo();
            foreach (var item in driveInfo)
            {
                foreach (var item2 in drive.Partitions)
                {
                    if (item.Name == item2.MountPoint)
                    {
                        var usedSize = item.AvailableFreeSpace.ToByteSizeHuman();
                        var size = item.TotalSize.ToByteSizeHuman();

                        model.Partitions.Add(new PikaDriveDetailViewModelPartition
                        {
                            FileSystemType = item2.Type,
                            Label = item2.Label,
                            Uuid = item2.Uuid,
                            MountPoint = item.Name,
                            PartitionName = item2.Name,
                            PartitionPath = item2.Path,
                            Capacity = $"{usedSize} / {size}"
                        });
                    }
                }

            }

            return View(model);
        }
    }
}
