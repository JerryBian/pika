using ExecDotnet;
using Microsoft.Extensions.Logging;
using Pika.Common.Model;
using Pika.Common.Store;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Pika.Common.Command;

public class CommandManager : ICommandManager
{
    private readonly ConcurrentDictionary<long, CancellationTokenSource> _commandClients;
    private readonly string _completedLiteral;
    private readonly IPikaStore _dbRepository;
    private readonly ILogger<CommandManager> _logger;
    private readonly BatchBlock<PikaScriptRunOutput> _batchBlock;
    private readonly TransformBlock<PikaScriptRunOutput, PikaScriptRunOutput> _batchTimerBlock;
    private readonly ActionBlock<PikaScriptRunOutput[]> _writeLogBlock;
    private readonly SemaphoreSlim _semaphoreSlim;
    private readonly IServiceProvider _serviceProvider;

    public CommandManager(IPikaStore repository, ILogger<CommandManager> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _dbRepository = repository;
        _serviceProvider = serviceProvider;
        _completedLiteral = Guid.NewGuid().ToString();
        _semaphoreSlim = new SemaphoreSlim(1, 1);
        _commandClients = new ConcurrentDictionary<long, CancellationTokenSource>();
        _batchBlock = new BatchBlock<PikaScriptRunOutput>(10, new GroupingDataflowBlockOptions { EnsureOrdered = false });
        Timer batchTimer = new(x =>
        {
            _batchBlock.TriggerBatch();
        });
        _batchTimerBlock = new TransformBlock<PikaScriptRunOutput, PikaScriptRunOutput>(x =>
        {
            _ = batchTimer.Change(TimeSpan.FromSeconds(2), Timeout.InfiniteTimeSpan);
            return x;
        }, new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 1, MaxMessagesPerTask = 1000 });
        _writeLogBlock = new ActionBlock<PikaScriptRunOutput[]>(ProcessRunOutputAsync, new ExecutionDataflowBlockOptions { EnsureOrdered = false, MaxDegreeOfParallelism = 1 });
        _ = _batchTimerBlock.LinkTo(_batchBlock, new DataflowLinkOptions { PropagateCompletion = true });
        _ = _batchBlock.LinkTo(_writeLogBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task StopAllAsync()
    {
        foreach (var item in _commandClients)
        {
            item.Value.Cancel();
        }

        _batchTimerBlock.Complete();
        await _writeLogBlock.Completion;
    }

    public void Stop(long runId)
    {
        if (_commandClients.TryGetValue(runId, out var commandClient))
        {
            commandClient.Cancel();
            _ = _commandClients.TryRemove(runId, out _);
        }
    }

    public async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        List<Task> tasks = [];
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
                            var cts = new CancellationTokenSource();
                            var stopped = false;
                            _ = _commandClients.TryAdd(run.Id, cts);
                            var execOption = new ExecOption
                            {
                                IsStreamed = true,
                                Shell = run.ShellName,
                                ShellExtension = run.ShellExt,
                                ShellParameter = run.ShellOption,
                                ErrorDataReceivedHandler = async m =>
                                {
                                    PikaScriptRunOutput output = new()
                                    {
                                        TaskRunId = run.Id,
                                        IsError = true,
                                        Message = m,
                                        CreatedAt = DateTime.Now.Ticks
                                    };

                                    _ = await _batchTimerBlock.SendAsync(output);
                                    _logger.LogError(m);

                                },
                                OutputDataReceivedHandler = async m =>
                                {
                                    PikaScriptRunOutput output = new()
                                    {
                                        TaskRunId = run.Id,
                                        IsError = false,
                                        Message = m,
                                        CreatedAt = DateTime.Now.Ticks
                                    };

                                    _ = await _batchTimerBlock.SendAsync(output);
                                    _logger.LogInformation(m);
                                    await Task.CompletedTask;
                                },
                                OnCancelledHandler = async () =>
                                {
                                    stopped = true;
                                    await Task.CompletedTask;
                                }
                            };
                            await Exec.RunAsync(run.Script, execOption, cts.Token);

                            PikaScriptRunOutput output = new()
                            {
                                TaskRunId = run.Id,
                                IsError = stopped,
                                Message = _completedLiteral,
                                CreatedAt = DateTime.Now.Ticks
                            };

                            _ = await _batchTimerBlock.SendAsync(output);
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

    private async Task ProcessRunOutputAsync(PikaScriptRunOutput[] output)
    {
        try
        {
            var nonCompletedOutputs = output.Where(x => x.Message != _completedLiteral);
            var completedOutputs = output.Except(nonCompletedOutputs);

            await _dbRepository.AddScriptRunOutputAsync(nonCompletedOutputs.ToList());

            foreach (var item in completedOutputs)
            {
                var status = item.IsError ? PikaScriptStatus.Stopped : PikaScriptStatus.Completed;
                await _dbRepository.UpdateScriptRunStatusAsync(item.TaskRunId, status);
                _logger.LogInformation($"Run {item.TaskRunId} marked as {status}.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Process output failed. {output.Length} output affected.");
        }
    }

    private async Task<PikaScriptRun> GetPendingTaskAsync(CancellationToken cancellationToken)
    {
        await _semaphoreSlim.WaitAsync(cancellationToken);
        try
        {
            var pendingRuns =
                await _dbRepository.GetScriptRunsAsync(whereClause: $"status={(int)PikaScriptStatus.Pending}",
                    orderByClause: "created_at ASC");
            var run = pendingRuns.FirstOrDefault();
            if (run != null)
            {
                await _dbRepository.UpdateScriptRunStatusAsync(run.Id, PikaScriptStatus.Running);
            }

            return run;
        }
        finally
        {
            _ = _semaphoreSlim.Release();
        }
    }
}