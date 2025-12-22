using ExecDotnet;
using Pika.Common.Store;
using Pika.Common.Util;
using System.Text.Json;

namespace Pika.Common.Drive
{
    public class PikaDriveOps : IPikaDriveOps
    {
        private readonly IDbRepository _repository;
        private readonly ILogger<PikaDriveOps> _logger;

        public PikaDriveOps(IDbRepository repository, ILogger<PikaDriveOps> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task UpdatePikaDriveAsync()
        {
            var lastLsblk = await RunPikaDriveLsblkAsync();
            if (lastLsblk == null)
            {
                return;
            }

            var diskDevices = lastLsblk.BlockDevices.Where(x => x.Type == "disk").OrderBy(x => x.Name).ToList();
            var pikaDrives = diskDevices.Select(x =>
            {
                var d = new PikaDriveTable
                {
                    Id = x.Id,
                    Name = x.Name,
                    Model = x.Model,
                    Path = x.Path,
                    Serial = x.Serial,
                    Size = x.Size,
                    Tran = x.Tran,
                    Type = x.Rota ? "HDD" : "SSD"
                };
                foreach (var item in x.Children)
                {
                    d.Partitions.Add(new PikaDrivePartitionTable
                    {
                        DriveId = x.Id,
                        Label = item.Label,
                        MountPoint = item.MountPoint,
                        Name = item.Name,
                        Path = item.Path,
                        Size = item.Size,
                        Type = item.FsType,
                        Uuid = item.Uuid
                    });
                }
                return d;
            }).ToList();

            await _repository.AddOrUpdatePikaDrivesAsync(pikaDrives);
        }

        public List<DriveInfo> GetDriveInfo()
        {
            return DriveInfo.GetDrives().Where(x => x.IsReady).ToList();
        }

        public async Task<List<PikaDriveTable>> GetActivePikaDrivesAsync()
        {
            var result = await _repository.GetPikaDrivesAsync();
            return result.OrderBy(x => x.Name).ToList();
        }

        public async Task<PikaDrivePowerStatus> GetDevicePowerStatusAsync(string devicePath)
        {
            var status = PikaDrivePowerStatus.Unknown;
            var command = $"hdparm -C {devicePath}";
            var execResult = await Exec.RunAsync(command);
            if (execResult.ExitCode != 0)
            {
                _logger.LogError($"Failed to get power status for device {devicePath} via hdparm: {execResult.Output}");
            }
            else
            {
                var output = execResult.Output.Trim();
                if (output.Contains("standby", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("sleep", StringComparison.OrdinalIgnoreCase))
                {
                    status = PikaDrivePowerStatus.Standby;
                }

                if (output.Contains("active", StringComparison.OrdinalIgnoreCase) ||
                    output.Contains("idle", StringComparison.OrdinalIgnoreCase))
                {
                    status = PikaDrivePowerStatus.Active;
                }
            }

            if (status == PikaDrivePowerStatus.Unknown)
            {
                execResult = await Exec.RunAsync($"smartctl -i -n standby -j {devicePath}");
                if (execResult.ExitCode != 0)
                {
                    _logger.LogError($"Failed to get power status for device {devicePath} via smartctl, exitCode = {execResult.ExitCode}.");
                    if (execResult.ExitCode == 2)
                    {
                        status = PikaDrivePowerStatus.Standby;
                    }
                }
                else
                {
                    var smartctlObj = JsonUtil.Deserialize<PikaDriveSmartctl>(execResult.Output);
                    status = smartctlObj.Smartctl.ExitStatus == 2 ? PikaDrivePowerStatus.Standby : PikaDrivePowerStatus.Active;
                }
            }

            return status;
        }

        public async Task<PikaDriveLsblk> RunPikaDriveLsblkAsync()
        {
            const string command = "lsblk -O -J";
            var execResult = await Exec.RunAsync(command);
            if (execResult.ExitCode != 0)
            {
                _logger.LogError($"Failed to run lsblk command: {execResult.Output}");
                return null;
            }

            return JsonUtil.Deserialize<PikaDriveLsblk>(execResult.Output);
        }

        public async Task<PikaDriveSmartctlTable> RunPikaDriveSmartctlAsync(string drivePath)
        {
            var activeDrives = await _repository.GetPikaDrivesAsync();
            var drive = activeDrives.FirstOrDefault(x => x.Path == drivePath);
            if (drive == null)
            {
                _logger.LogWarning($"No active drive found for drive {drivePath}");
                return null;
            }

            var command = $"smartctl -n standby -H -A -j {drivePath}";
            var execResult = await Exec.RunAsync(command);
            if (execResult.ExitCode != 0)
            {
                _logger.LogError($"Failed to run smartctl command: {execResult.Output}");
                return null;
            }

            var jsonDoc = JsonUtil.DeserializeAsDoc(execResult.Output);
            var exitStatus = jsonDoc.RootElement.GetProperty("smartctl").GetProperty("exit_status").GetInt32();
            if (exitStatus != 0)
            {
                _logger.LogError($"Command smartctl returned non-zero status: {execResult.Output}");
                return null;
            }

            var attributes = GetAttributes(jsonDoc.RootElement);
            var smartctlTable = new PikaDriveSmartctlTable
            {
                DriveId = drive.Id,
                Passed = jsonDoc.RootElement.GetProperty("smart_status").GetProperty("passed").GetBoolean(),
                CommandTimeout = attributes.TryGetValue(188, out var val) ? val.RawValue : null,
                CurrentPendingSector = attributes.TryGetValue(197, out var cps) ? cps.RawValue : null,
                LoadCycleCount = attributes.TryGetValue(193, out var lcc) ? lcc.RawValue : null,
                OfflineUncorrectable = attributes.TryGetValue(198, out var ou) ? ou.RawValue : null,
                PowerCycleCount = attributes.TryGetValue(12, out var pcc) ? pcc.RawValue : null,
                PowerOffRestartCount = attributes.TryGetValue(192, out var porc) ? porc.RawValue : null,
                PowerOnHours = attributes.TryGetValue(9, out var poh) ? poh.RawValue : null,
                ReallocatedSectorCt = attributes.TryGetValue(5, out var rsc) ? rsc.RawValue : null,
                ReportedUncorrect = attributes.TryGetValue(187, out var ru) ? ru.RawValue : null,
                StartStopCount = attributes.TryGetValue(4, out var ssc) ? ssc.RawValue : null,
                UdmaCrcErrorCount = attributes.TryGetValue(199, out var ucec) ? ucec.RawValue : null,
                Temperature = attributes.TryGetValue(194, out var temp) ? temp.RawValue : null
            };

            await _repository.AddPikaDriveSmartctlAsync(smartctlTable);
            return smartctlTable;
        }

        public async Task<List<PikaDriveSmartctlTable>> GetRecentPikaDriveSmartctlAsync(string driveId)
        {
            return await _repository.GetPikaDriveSmartctlTablesAsync(driveId, DateTime.Now.AddMonths(-3));
        }

        private IDictionary<int, PikaDriveSmartctlAttribute> GetAttributes(JsonElement root)
        {
            var result = new Dictionary<int, PikaDriveSmartctlAttribute>();
            var table = root
                .GetProperty("ata_smart_attributes")
                .GetProperty("table");
            foreach (var attr in table.EnumerateArray())
            {
                if (!attr.TryGetProperty("id", out var ele))
                {
                    continue;
                }

                if (ele.TryGetInt32(out var id))
                {
                    var value = attr.GetProperty("value").GetInt64();
                    var rawValue = attr.GetProperty("raw").GetProperty("value").GetInt64();
                    var rawString = attr.GetProperty("raw").GetProperty("string").GetString();
                    result[id] = new PikaDriveSmartctlAttribute
                    {
                        Id = id,
                        Value = value,
                        RawValue = rawValue,
                        RawString = rawString
                    };
                }
            }

            return result;
        }

        private struct PikaDriveSmartctlAttribute
        {
            public int Id { get; set; }

            public long Value { get; set; }

            public long RawValue { get; set; }

            public string RawString { get; set; }
        }
    }
}
