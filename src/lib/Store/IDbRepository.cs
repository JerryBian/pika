using Pika.Lib.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pika.Lib.Store;

public interface IDbRepository
{
    Task StartupAsync();

    Task<long> AddTaskAsync(PikaTask task);

    Task UpdateTaskAsync(PikaTask task);

    Task<PikaTask> GetTaskAsync(long taskId);

    Task<List<PikaTask>> GetTasksAsync(int limit = 0, int offset = -1, string whereClause = "",
        string orderByClause = "");

    Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit = 0, int offset = -1, string whereClause = "",
        string orderByClause = "");

    Task<PikaSystemStatus> GetSystemStatusAsync();

    Task<long> AddTaskRunAsync(PikaTaskRun run);

    Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput);

    Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, long laterThan = default, int limit = -1,
        bool desc = true);

    Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status);

    Task<PikaTaskRun> GetTaskRunAsync(long runId);

    Task<string> GetSetting(string key);

    Task InsertOrUpdateSetting(string key, string value);

    Task<int> GetTasksCountAsync();

    Task DeleteTaskAsync(long taskId);

    Task<int> GetRunsCountAsync(string whereClause = "");

    Task<int> GetRunsCountAsync(long taskId);

    long GetDbSize();

    Task VacuumDbAsync();
}