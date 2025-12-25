using Pika.Common.Model;

namespace Pika.Common.Store
{
    public interface IPikaStore
    {
        Task<long> AddAppAsync(PikaApp app);
        Task AddOrUpdatePikaDrivesAsync(List<PikaDriveTable> drives);
        Task AddPikaDriveSmartctlAsync(PikaDriveSmartctlTable pikaDriveSmartctl);
        Task<long> AddScriptAsync(PikaScript script);
        Task<long> AddScriptRunAsync(PikaScriptRun run);
        Task AddScriptRunOutputAsync(List<PikaScriptRunOutput> runOutputs);
        Task AddScriptRunOutputAsync(PikaScriptRunOutput runOutput);
        Task DeleteAppAsync(long appId);
        Task DeleteScriptAsync(long scriptId);
        Task<PikaApp> GetAppAsync(long appId);
        Task<List<PikaApp>> GetAppsAsync();
        long GetDbSize();
        Task<List<PikaDriveTable>> GetPikaDrivesAsync();
        Task<List<PikaDriveSmartctlTable>> GetPikaDriveSmartctlTablesAsync(string driveId, DateTime from);
        Task<PikaDriveSmartctlTable> GetPikaDriveSmartTableAsync(string driveId);
        Task<int> GetRunsCountAsync(string whereClause = "");
        Task<int> GetRunsCountAsync(long scriptId);
        Task<PikaScript> GetScriptAsync(long scriptId);
        Task<PikaScriptRun> GetScriptRunAsync(long runId);
        Task<List<PikaScriptRunOutput>> GetScriptRunOutputs(long scriptRunId, long laterThan = 0, int limit = -1, bool desc = true);
        Task<List<PikaScriptRun>> GetScriptRunsAsync(int limit = 0, int offset = -1, string whereClause = "", string orderByClause = "");
        Task<List<PikaScript>> GetScriptsAsync(int limit = 0, int offset = -1, string whereClause = "", string orderByClause = "");
        Task<string> GetSetting(string key);
        Task<int> GetTasksCountAsync();
        Task InsertOrUpdateSetting(string key, string value);
        Task StartupAsync();
        Task UpdateAppAsync(PikaApp app);
        Task UpdateScriptAsync(PikaScript script);
        Task UpdateScriptRunStatusAsync(long runId, PikaScriptStatus status);
        Task VacuumDbAsync();
    }
}