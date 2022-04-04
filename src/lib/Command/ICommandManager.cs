using System.Threading;
using System.Threading.Tasks;

namespace Pika.Lib.Command;

public interface ICommandManager
{
    void Stop(long runId);

    void StopAll();

    Task ExecuteAsync(CancellationToken stoppingToken);
}