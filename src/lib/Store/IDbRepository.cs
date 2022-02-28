using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pika.Lib.Model;

namespace Pika.Lib.Store;

public interface IDbRepository
{
    Task StartupAsync();

    Task<long> AddTaskAsync(PikaTask task);

    Task UpdateTaskAsync(PikaTask task);

    Task<PikaTask> GetTaskAsync(long taskId);

    Task<List<PikaTask>> GetTasksAsync(int limit, int offset, string whereClause = "", string orderByClause = "");

    Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit, int offset, string whereClause = "",
        string orderByClause = "");

    Task<PikaSystemStatus> GetSystemStatusAsync();

    Task<long> AddTaskRunAsync(long taskId);

    Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput);

    Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, DateTime laterThan);

    Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status);

    Task<PikaTaskRun> GetTaskRunAsync(long runId);
}