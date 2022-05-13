using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pika.Lib.Command;

namespace Pika.Web.HostedServices;

public class TaskHostedService : BackgroundService
{
    private readonly ICommandManager _commandManager;

    public TaskHostedService(ICommandManager commandManager)
    {
        _commandManager = commandManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _commandManager.ExecuteAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _commandManager.StopAllAsync();
        await Task.CompletedTask;
    }
}