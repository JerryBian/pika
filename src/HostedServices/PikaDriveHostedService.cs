
using Pika.Common.Drive;
using Pika.Common.Extension;
using Pika.Common.Model;

namespace Pika.HostedServices
{
    public class PikaDriveHostedService : BackgroundService
    {
        private readonly IPikaDriveOps _pikaDriveOps;
        private readonly ILogger<PikaDriveHostedService> _logger;
        private readonly IDictionary<string, DateTime> _lastRunAtDict;

        public PikaDriveHostedService(
            IPikaDriveOps pikaDriveOps, 
            ILogger<PikaDriveHostedService> logger)
        {
            _pikaDriveOps = pikaDriveOps;
            _logger = logger;
            _lastRunAtDict = new Dictionary<string, DateTime>();
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _pikaDriveOps.UpdatePikaDriveAsync();

            foreach(var drive in await _pikaDriveOps.GetActivePikaDrivesAsync())
            {
                _lastRunAtDict[drive.Id] = DateTime.MinValue;
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var drives = await _pikaDriveOps.GetActivePikaDrivesAsync();
                    foreach(var drive in drives)
                    {
                        var powerStatus = await _pikaDriveOps.GetDevicePowerStatusAsync(drive.Path);
                        _logger.LogInformation($"Drive {drive.Name} (ID: {drive.Id}) is {powerStatus}");

                        if(powerStatus == PikaDrivePowerStatus.Active)
                        {
                            if(_lastRunAtDict.TryGetValue(drive.Id, out var lastRunAt))
                            {
                                var timeSinceLastRun = DateTime.Now - lastRunAt;
                                if(timeSinceLastRun > TimeSpan.FromDays(3))
                                {
                                    await _pikaDriveOps.RunPikaDriveSmartctlAsync(drive.Path);
                                    _logger.LogInformation($"Refreshed drive info for {drive.Name} (ID: {drive.Id})");
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in PikaDriveHostedService.");
                }

                await Task.Delay(TimeSpan.FromMinutes(8), stoppingToken).OkForCancel();
            }
        }
    }
}
