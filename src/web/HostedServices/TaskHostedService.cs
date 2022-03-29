using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Pika.Lib.Command;
using Pika.Lib.Model;
using Pika.Lib.Store;

namespace Pika.Web.HostedServices;

public class TaskHostedService : BackgroundService
{
    private readonly ICommandClient _commandClient;
    private readonly IDbRepository _dbRepository;
    private readonly SemaphoreSlim _semaphoreSlim;

    public TaskHostedService(IDbRepository dbRepository, ICommandClient commandClient)
    {
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _dbRepository = dbRepository;
        _commandClient = commandClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var run = await GetPendingTaskAsync(stoppingToken);
                        if (run != null)
                        {
                            await _commandClient.RunAsync(run.Script, async m =>
                            {
                                var output = new PikaTaskRunOutput {TaskRunId = run.Id, IsError = false, Message = m};
                                await _dbRepository.AddTaskRunOutputAsync(output);
                            }, async m =>
                            {
                                var output = new PikaTaskRunOutput {TaskRunId = run.Id, IsError = true, Message = m};
                                await _dbRepository.AddTaskRunOutputAsync(output);
                            }, stoppingToken);
                            await _dbRepository.UpdateTaskRunStatusAsync(run.Id, PikaTaskStatus.Completed);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }, stoppingToken));
        }

        await Task.WhenAll(tasks);
    }

    private async Task<PikaTaskRun> GetPendingTaskAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var pendingRuns = await _dbRepository.GetTaskRunsAsync(1, 0,
                $"status={(int) PikaTaskStatus.Pending}", "created_at ASC");
            var run = pendingRuns.FirstOrDefault();
            if (run != null)
            {
                await _dbRepository.UpdateTaskRunStatusAsync(run.Id, PikaTaskStatus.Running);
            }

            return run;
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}