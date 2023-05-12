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
using System.Threading.Tasks.Dataflow;

namespace Pika.Lib.Command;

public class CommandManager : ICommandManager
{
    private readonly ConcurrentDictionary<long, ICommandClient> _commandClients;
    private readonly string _completedLiteral;
    private readonly IDbRepository _dbRepository;
    private readonly ILogger<CommandManager> _logger;
    private readonly BatchBlock<PikaTaskRunOutput> _batchBlock;
    private readonly TransformBlock<PikaTaskRunOutput, PikaTaskRunOutput> _batchTimerBlock;
    private readonly ActionBlock<PikaTaskRunOutput[]> _writeLogBlock;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly IServiceProvider _serviceProvider;

    public CommandManager(IDbRepository repository, ILogger<CommandManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _dbRepository = repository;
        _serviceProvider = serviceProvider;
        _completedLiteral = Guid.NewGuid().ToString();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _commandClients = new ConcurrentDictionary<long, ICommandClient>();
        _batchBlock = new BatchBlock<PikaTaskRunOutput>(100, new GroupingDataflowBlockOptions { EnsureOrdered = false });
        var batchTimer = new Timer(x =>
        {
            _batchBlock.TriggerBatch();
        });
        _batchTimerBlock = new TransformBlock<PikaTaskRunOutput, PikaTaskRunOutput>(x =>
        {
            batchTimer.Change(TimeSpan.FromSeconds(3), Timeout.InfiniteTimeSpan);
            return x;
        }, new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 1, MaxMessagesPerTask = 1000 });
        _writeLogBlock = new ActionBlock<PikaTaskRunOutput[]>(ProcessRunOutputAsync, new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 1 });
        _batchTimerBlock.LinkTo(_batchBlock, new DataflowLinkOptions { PropagateCompletion = true });
        _batchBlock.LinkTo(_writeLogBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task StopAllAsync()
    {
        foreach (KeyValuePair<long, ICommandClient> item in _commandClients)
        {
            item.Value.Stop();
        }

        _batchTimerBlock.Complete();
        await _writeLogBlock.Completion;
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

                                    await _batchTimerBlock.SendAsync(output);
                                    _logger.LogInformation(m);
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

                                    await _batchTimerBlock.SendAsync(output);
                                    _logger.LogError(m);

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

                            await _batchTimerBlock.SendAsync(output);
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

        await Task.WhenAll(tasks);
    }

    private async Task ProcessRunOutputAsync(PikaTaskRunOutput[] output)
    {
        try
        {
            var nonCompletedOutputs = output.Where(x => x.Message != _completedLiteral);
            var completedOutputs = output.Except(nonCompletedOutputs);

            await _dbRepository.AddTaskRunOutputAsync(nonCompletedOutputs.ToList());

            foreach (var item in completedOutputs)
            {
                PikaTaskStatus status = item.IsError ? PikaTaskStatus.Stopped : PikaTaskStatus.Completed;
                await _dbRepository.UpdateTaskRunStatusAsync(item.TaskRunId, status);
                _logger.LogInformation($"Run {item.TaskRunId} marked as {status}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Process output failed. {output.Length} output affected.");
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