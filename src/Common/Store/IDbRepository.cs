using Pika.Common.Drive;
using Pika.Common.Model;

namespace Pika.Common.Store
{
    public interface IDbRepository
    {
        Task<long> AddAppAsync(PikaApp app);
        Task AddOrUpdatePikaDrivesAsync(List<PikaDriveTable> drives);
        Task AddPikaDriveSmartctlAsync(PikaDriveSmartctlTable pikaDriveSmartctl);
        Task<long> AddTaskAsync(PikaTask task);
        Task<long> AddTaskRunAsync(PikaTaskRun run);
        Task AddTaskRunOutputAsync(List<PikaTaskRunOutput> runOutputs);
        Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput);
        Task DeleteAppAsync(long appId);
        Task DeleteTaskAsync(long taskId);
        Task<PikaApp> GetAppAsync(long appId);
        Task<List<PikaApp>> GetAppsAsync(int limit = 0, int offset = -1, string whereClause = "", string orderByClause = "");
        long GetDbSize();
        Task<List<PikaDriveTable>> GetPikaDrivesAsync();
        Task<List<PikaDriveSmartctlTable>> GetPikaDriveSmartctlTablesAsync(string driveId, DateTime from);
        Task<PikaDriveSmartctlTable> GetPikaDriveSmartTableAsync(string driveId);
        Task<int> GetRunsCountAsync(string whereClause = "");
        Task<int> GetRunsCountAsync(long taskId);
        Task<string> GetSetting(string key);
        Task<PikaSystemStatus> GetSystemStatusAsync();
        Task<PikaTask> GetTaskAsync(long taskId);
        Task<PikaTaskRun> GetTaskRunAsync(long runId);
        Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, long laterThan = 0, int limit = -1, bool desc = true);
        Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit = 0, int offset = -1, string whereClause = "", string orderByClause = "");
        Task<List<PikaTask>> GetTasksAsync(int limit = 0, int offset = -1, string whereClause = "", string orderByClause = "");
        Task<int> GetTasksCountAsync();
        Task InsertOrUpdateSetting(string key, string value);
        Task StartupAsync();
        Task UpdateAppAsync(PikaApp app);
        Task UpdateTaskAsync(PikaTask task);
        Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status);
        Task VacuumDbAsync();
    }
}