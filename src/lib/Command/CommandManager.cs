using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pika.Lib.Model;
using Pika.Lib.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pika.Lib.Command;

public class CommandManager : ICommandManager
{
    private readonly ConcurrentDictionary<long, ICommandClient> _commandClients;
    private readonly string _completedLiteral;
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
        _completedLiteral = Guid.NewGuid().ToString();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _queue = new ConcurrentQueue<PikaTaskRunOutput>();
        _commandClients = new ConcurrentDictionary<long, ICommandClient>();
    }

    public async Task StopAllAsync()
    {
        foreach (KeyValuePair<long, ICommandClient> item in _commandClients)
        {
            item.Value.Stop();
        }

        var outputs = new List<PikaTaskRunOutput>();
        while (_queue.TryDequeue(out PikaTaskRunOutput output))
        {
            outputs.Add(output);
        }

        await ProcessRunOutputAsync(outputs);
    }

    public void Stop(long runId)
    {
        if (_commandClients.TryGetValue(runId, out ICommandClient commandClient))
        {
            commandClient.Stop();
            _ = _commandClients.TryRemove(runId, out _);
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Task> tasks = new();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        PikaTaskRun run = await GetPendingTaskAsync(stoppingToken);
                        if (run != null)
                        {
                            await using ICommandClient commandClient = _serviceProvider.GetService<ICommandClient>();
                            if (commandClient == null)
                            {
                                continue;
                            }

                            bool stopped = false;
                            _ = _commandClients.TryAdd(run.Id, commandClient);
                            await commandClient.RunAsync(run.ShellName, run.ShellOption, run.ShellExt, run.Script,
                                async m =>
                                {
                                    PikaTaskRunOutput output = new()
                                    {
                                        TaskRunId = run.Id,
                                        IsError = false,
                                        Message = m,
                                        CreatedAt = DateTime.Now.Ticks
                                    };
                                    _queue.Enqueue(output);
                                    await Task.CompletedTask;
                                }, async m =>
                                {
                                    PikaTaskRunOutput output = new()
                                    {
                                        TaskRunId = run.Id,
                                        IsError = true,
                                        Message = m,
                                        CreatedAt = DateTime.Now.Ticks
                                    };
                                    _queue.Enqueue(output);
                                    await Task.CompletedTask;
                                }, async () =>
                                {
                                    stopped = true;
                                    await Task.CompletedTask;
                                });

                            PikaTaskRunOutput output = new()
                            {
                                TaskRunId = run.Id,
                                IsError = stopped,
                                Message = _completedLiteral,
                                CreatedAt = DateTime.Now.Ticks
                            };
                            _queue.Enqueue(output);
                            _ = _commandClients.TryRemove(run.Id, out _);
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
                var outputs = new List<PikaTaskRunOutput>();
                while (_queue.TryDequeue(out PikaTaskRunOutput output))
                {
                    outputs.Add(output);
                }

                await ProcessRunOutputAsync(outputs);

                if(!outputs.Any())
                {
                    await Task.Delay(100, stoppingToken);
                }
            }
        }, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessRunOutputAsync(List<PikaTaskRunOutput> output)
    {
        try
        {
            var nonCompletedOutputs = output.Where(x => x.Message != _completedLiteral);
            var completedOutputs = output.Except(nonCompletedOutputs);

            await _dbRepository.AddTaskRunOutputAsync(nonCompletedOutputs.ToList());

            foreach(var item in completedOutputs)
            {
                PikaTaskStatus status = item.IsError ? PikaTaskStatus.Stopped : PikaTaskStatus.Completed;
                await _dbRepository.UpdateTaskRunStatusAsync(item.TaskRunId, status);
                _logger.LogInformation($"Run {item.TaskRunId} marked as {status}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Process output failed. {output.Count} output affected.");
        }
    }

    private async Task<PikaTaskRun> GetPendingTaskAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            List<PikaTaskRun> pendingRuns =
                await _dbRepository.GetTaskRunsAsync(whereClause: $"status={(int)PikaTaskStatus.Pending}",
                    orderByClause: "created_at ASC");
            PikaTaskRun run = pendingRuns.FirstOrDefault();
            if (run != null)
            {
                await _dbRepository.UpdateTaskRunStatusAsync(run.Id, PikaTaskStatus.Running);
            }

            return run;
        }
        finally
        {
            _ = _semaphoreSlim.Release();
        }
    }
}