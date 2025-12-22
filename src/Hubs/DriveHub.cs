using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Pika.Common.Drive;
using Pika.Common.Extension;
using Pika.Models;
using System.Runtime.CompilerServices;

namespace Pika.Hubs
{
    [Route("/hub/drive")]
    public class DriveHub : Hub
    {
        private readonly ILogger<DriveHub> _logger;
        private readonly IPikaDriveOps _pikaDriveOps;

        public DriveHub(IPikaDriveOps pikaDriveOps, ILogger<DriveHub> logger)
        {
            _pikaDriveOps = pikaDriveOps;
            _logger = logger;
        }

        public async IAsyncEnumerable<PikaDrivePowerStateViewModel> GetDrivePowerStatus(
            string drivePath, 
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while(!cancellationToken.IsCancellationRequested)
            {
                var status = await _pikaDriveOps.GetDevicePowerStatusAsync(drivePath);
                var result = new PikaDrivePowerStateViewModel
                {
                    StateString = status.ToString()
                };

                switch (status)
                {
                    case PikaDrivePowerStatus.Unknown:
                        result.ClassName = "text-dark";
                        break;
                    case PikaDrivePowerStatus.Standby:
                        result.ClassName = "text-warning";
                        break;
                    case PikaDrivePowerStatus.Active:
                        result.ClassName = "text-success";
                        break;
                    default:
                        result.ClassName = "text-secondary";
                        break;
                }

                yield return result;

                await Task.Delay(5000, cancellationToken).OkForCancel();
            }
        }
    }
}
