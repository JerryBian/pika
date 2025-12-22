using System.Threading;
using System.Threading.Tasks;

namespace Pika.Common.Command;

public interface ICommandManager
{
    void Stop(long runId);

    Task StopAllAsync();

    Task ExecuteAsync(CancellationToken stoppingToken);
}