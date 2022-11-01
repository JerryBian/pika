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

        while (_queue.TryDequeue(out PikaTaskRunOutput output))
        {
            await ProcessRunOutputAsync(output);
        }
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
                if (_queue.TryDequeue(out PikaTaskRunOutput output))
                {
                    await ProcessRunOutputAsync(output);
                    continue;
                }

                await Task.Delay(100, stoppingToken);
            }
        }, stoppingToken));
        await Task.WhenAll(tasks);
    }

    private async Task ProcessRunOutputAsync(PikaTaskRunOutput output)
    {
        try
        {
            if (output.Message != _completedLiteral)
            {
                await _dbRepository.AddTaskRunOutputAsync(output);

                if (output.IsError)
                {
                    _logger.LogError($"Run: {output.TaskRunId}, Message: {output.Message}");
                }
                else
                {
                    _logger.LogInformation($"Run: {output.TaskRunId}, Message: {output.Message}");
                }
            }
            else
            {
                PikaTaskStatus status = output.IsError ? PikaTaskStatus.Stopped : PikaTaskStatus.Completed;
                await _dbRepository.UpdateTaskRunStatusAsync(output.TaskRunId, status);
                _logger.LogInformation($"Run {output.TaskRunId} marked as {status}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                $"Process output failed. Run: {output.Id}, IsError: {output.IsError}, Message: {output.Message}");
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