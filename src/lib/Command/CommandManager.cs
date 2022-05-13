using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pika.Lib.Model;
using Pika.Lib.Store;

namespace Pika.Lib.Command;

public class CommandManager : ICommandManager
{
    private readonly ConcurrentDictionary<long, ICommandClient> _commandClients;
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<CommandManager> _logger;
    private readonly ConcurrentQueue<PikaTaskRunOutput> _queue;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly IServiceProvider _serviceProvider;

    public CommandManager(IDbRepository repository, ILogger<CommandManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _dbRepository = repository;
        _serviceProvider = serviceProvider;
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _queue = new ConcurrentQueue<PikaTaskRunOutput>();
        _commandClients = new ConcurrentDictionary<long, ICommandClient>();
    }

    public async Task StopAllAsync()
    {
        foreach (var item in _commandClients)
        {
            item.Value.Stop();
        }

        while (_queue.TryDequeue(out var output))
        {
            await _dbRepository.AddTaskRunOutputAsync(output);
        }
    }

    public void Stop(long runId)
    {
        if (_commandClients.TryGetValue(runId, out var commandClient))
        {
            commandClient.Stop();
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
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
                            await using var commandClient = _serviceProvider.GetService<ICommandClient>();
                            if (commandClient == null)
                            {
                                continue;
                            }

                            var stopped = false;
                            _commandClients.TryAdd(run.Id, commandClient);
                            await commandClient.RunAsync(run.ShellName, run.ShellOption, run.ShellExt, run.Script,
                                async m =>
                                {
                                    var output = new PikaTaskRunOutput
                                        {TaskRunId = run.Id, IsError = false, Message = m};
                                    _queue.Enqueue(output);
                                    await Task.CompletedTask;
                                }, async m =>
                                {
                                    var output = new PikaTaskRunOutput
                                        {TaskRunId = run.Id, IsError = true, Message = m};
                                    _queue.Enqueue(output);
                                    await Task.CompletedTask;
                                }, async () =>
                                {
                                    stopped = true;
                                    await Task.CompletedTask;
                                });

                            await _dbRepository.UpdateTaskRunStatusAsync(run.Id,
                                stopped ? PikaTaskStatus.Stopped : PikaTaskStatus.Completed);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Task run failed.");
                    }
                }
            }, stoppingToken));
        }

        tasks.Add(Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var output))
                {
                    await _dbRepository.AddTaskRunOutputAsync(output);
                    continue;
                }

                await Task.Delay(100);
            }
        }, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task<PikaTaskRun> GetPendingTaskAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var pendingRuns =
                await _dbRepository.GetTaskRunsAsync(whereClause: $"status={(int) PikaTaskStatus.Pending}",
                    orderByClause: "created_at ASC");
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