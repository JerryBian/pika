using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pika.Common.Dapper;
using Pika.Common.Model;
using System.Text;

namespace Pika.Common.Store;

public class PikaStore : IPikaStore
{
    private readonly PikaOptions _options;
    private string _connectionString;
    private string _pikaDb;

    public PikaStore(IOptions<PikaOptions> options)
    {
        _options = options.Value;
        SqlMapper.AddTypeHandler(new DateTimeHandler());
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task StartupAsync()
    {
        if (string.IsNullOrEmpty(_options.DbLocation))
        {
            throw new Exception("Please specify the DB location.");
        }

        _ = Directory.CreateDirectory(_options.DbLocation);
        _pikaDb = Path.Combine(_options.DbLocation, "pika.db");
        if (File.Exists(_pikaDb) && _options.ForceRecreateDb)
        {
            File.Delete(_pikaDb);
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            Cache = SqliteCacheMode.Shared,
            DataSource = _pikaDb,
            Mode = SqliteOpenMode.ReadWriteCreate,
            DefaultTimeout = 60,
            Pooling = true,
            ForeignKeys = true
        };
        _connectionString = connectionStringBuilder.ToString();
        var baseDir = AppContext.BaseDirectory;
        if (string.IsNullOrEmpty(baseDir))
        {
            throw new Exception($"Cannot find base dir {baseDir}.");
        }

        var startupFile = Path.Combine(baseDir, "Common", "Store", "startup.sql");
        if (!File.Exists(startupFile))
        {
            throw new Exception($"Cannot find startup script: {startupFile}");
        }

        await using SqliteConnection connection = new(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = await File.ReadAllTextAsync(startupFile, Encoding.UTF8);
        await connection.OpenAsync();
        _ = await command.ExecuteNonQueryAsync();
    }

    #region App

    public async Task<List<PikaApp>> GetAppsAsync(int limit = 0, int offset = -1, string whereClause = "",
        string orderByClause = "")
    {
        await using SqliteConnection connection = new(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $" where {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        var limitClause = string.Empty;
        if (limit > 0 && offset >= 0)
        {
            limitClause = $"LIMIT {limit} OFFSET {offset}";
        }

        var sql = $"SELECT * FROM app {whereClause} {orderByClause} {limitClause}";
        return (await connection.QueryAsync<PikaApp>(sql)).AsList();
    }

    public async Task<PikaApp> GetAppAsync(long appId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.QuerySingleOrDefaultAsync<PikaApp>("SELECT * FROM app WHERE id=@id", new { id = appId });
        return result;
    }

    public async Task<long> AddAppAsync(PikaApp app)
    {
        await using SqliteConnection connection = new(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO app(name, description, init_script, init_script_path, start_script, start_script_path, stop_script, stop_script_path, state_script, state_script_path, running_state, port, shell_name, shell_option, shell_ext, created_at, last_modified_at) " +
            "VALUES(@name, @desc, @initScript, @initScriptPath, @startScript, @startScriptPath, @stopScript, @stopScriptPath, @stateScript, @stateScriptPath, @runningState, @port, @shellName, @shellOption, @shellExt, @createdAt, @lastModifiedAt) RETURNING id",
            new
            {
                name = app.Name,
                desc = app.Description,
                initScript = app.InitScript,
                initScriptPath = app.InitScriptPath,
                startScript = app.StartScript,
                startScriptPath = app.StartScriptPath,
                stopScript = app.StopScript,
                stopScriptPath = app.StopScriptPath,
                createdAt = app.CreatedAt,
                lastModifiedAt = app.LastModifiedAt,
                shellName = app.ShellName,
                shellOption = app.ShellOption,
                shellExt = app.ShellExt,
                stateScript = app.StateScript,
                stateScriptPath = app.StateScriptPath,
                runningState = app.RunningState,
                port = app.Port
            });
        return id == null ? -1 : Convert.ToInt64(id);
    }

    public async Task UpdateAppAsync(PikaApp app)
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE app " +
            "SET name=@name, " +
            "description=@desc, " +
            "shell_name=@shellName, " +
            "shell_option=@shellOption, " +
            "shell_ext=@shellExt, " +
            "init_script=@initScript, " +
            "init_script_path=@initScriptPath, " +
            "start_script=@startScript, " +
            "start_script_path=@startScriptPath, " +
            "stop_script=@stopScript, " +
            "stop_script_path=@stopScriptPath, " +
            "state_script=@stateScript, " +
            "state_script_path=@stateScriptPath, " +
            "running_state=@runningState, " +
            "port=@port, " +
            "last_modified_at=$lastModifiedAt WHERE id=$id",
            new
            {
                name = app.Name,
                desc = app.Description,
                initScript = app.InitScript,
                initScriptPath = app.InitScriptPath,
                startScript = app.StartScript,
                startScriptPath = app.StartScriptPath,
                stopScript = app.StopScript,
                stopScriptPath = app.StopScriptPath,
                lastModifiedAt = app.LastModifiedAt,
                shellName = app.ShellName,
                shellOption = app.ShellOption,
                shellExt = app.ShellExt,
                id = app.Id,
                stateScript = app.StateScript,
                stateScriptPath = app.StateScriptPath,
                runningState = app.RunningState,
                port = app.Port
            });
    }

    public async Task DeleteAppAsync(long appId)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "DELETE FROM app WHERE id=@appId", new
            {
                appId
            });
    }

    #endregion

    #region Script

    public async Task<long> AddScriptAsync(PikaScript script)
    {
        await using SqliteConnection connection = new(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO script(name, script, description, shell_name, shell_option, shell_ext, is_temp, created_at, last_modified_at) " +
            "VALUES(@name, @script, @desc, @shellName, @shellOption, @shellExt, @isTemp, @createdAt, @lastModifiedAt) RETURNING id",
            new
            {
                name = script.Name,
                script = script.Script,
                desc = script.Description,
                shellName = script.ShellName,
                shellOption = script.ShellOption,
                shellExt = script.ShellExt,
                isTemp = script.IsTemp,
                createdAt = script.CreatedAt,
                lastModifiedAt = script.LastModifiedAt
            });
        return id == null ? -1 : Convert.ToInt64(id);
    }

    public async Task UpdateScriptAsync(PikaScript script)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "UPDATE script SET name=@name, script=@script, description=@desc, shell_name=@shellName, shell_option=@shellOption, shell_ext=@shellExt, last_modified_at=@lastModifiedAt WHERE id=@id",
            new
            {
                name = script.Name,
                script = script.Script,
                desc = script.Description,
                id = script.Id,
                shellName = script.ShellName,
                shellOption = script.ShellOption,
                shellExt = script.ShellExt,
                lastModifiedAt = script.LastModifiedAt
            });
    }

    public async Task<int> GetRunsCountAsync(string whereClause = "")
    {
        await using SqliteConnection connection = new(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        var result =
            await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM script_run {whereClause}");
        return result;
    }

    public async Task<int> GetRunsCountAsync(long scriptId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM script_run WHERE script_id=@scriptId",
                new { scriptId });
        return result;
    }

    public async Task<int> GetTasksCountAsync()
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM task WHERE is_temp = 0");
        return result;
    }

    public async Task<PikaScript> GetScriptAsync(long scriptId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.QuerySingleOrDefaultAsync<PikaScript>("SELECT * FROM script WHERE id=@id", new { id = scriptId });
        return result;
    }

    public async Task DeleteScriptAsync(long scriptId)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "DELETE FROM script_run_output WHERE run_id IN(SELECT id FROM script_run WHERE script_id=@scriptId);" +
            "DELETE FROM script_run WHERE task_id=@scriptId;" +
            "DELETE FROM script WHERE id=@scriptId", new
            {
                scriptId
            });
    }

    public async Task<List<PikaScript>> GetScriptsAsync(int limit = 0, int offset = -1, string whereClause = "",
        string orderByClause = "")
    {
        await using SqliteConnection connection = new(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"AND {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        var limitClause = string.Empty;
        if (limit > 0 && offset >= 0)
        {
            limitClause = $"LIMIT {limit} OFFSET {offset}";
        }

        var sql = $"SELECT * FROM script WHERE is_temp = 0 {whereClause} {orderByClause} {limitClause}";
        return (await connection.QueryAsync<PikaScript>(sql)).AsList();
    }

    public async Task<List<PikaScriptRun>> GetScriptRunsAsync(int limit = 0, int offset = -1, string whereClause = "",
        string orderByClause = "")
    {
        await using SqliteConnection connection = new(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        var limitClause = string.Empty;
        if (limit > 0 && offset >= 0)
        {
            limitClause = $"LIMIT {limit} OFFSET {offset}";
        }

        var sql = $"SELECT * FROM script_run {whereClause} {orderByClause} {limitClause}";
        return (await connection.QueryAsync<PikaScriptRun>(sql)).AsList();
    }

    public async Task<long> AddScriptRunAsync(PikaScriptRun run)
    {
        await using SqliteConnection connection = new(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO script_run(script_id, status, script, shell_name, shell_option, shell_ext, created_at, started_at, completed_at) " +
            "VALUES(@scriptId, @status, @script, @shellName, @shellOption, @shellExt, @createdAt, 0, 0) RETURNING id",
            new
            {
                scriptId = run.ScriptId,
                status = (int)run.Status,
                script = run.Script,
                shellName = run.ShellName,
                shellOption = run.ShellOption,
                shellExt = run.ShellExt,
                createdAt = run.CreatedAt
            });
        return id == null ? -1 : Convert.ToInt64(id);
    }

    public async Task AddScriptRunOutputAsync(PikaScriptRunOutput runOutput)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "INSERT INTO script_run_output(run_id, message, is_error, created_at) VALUES(@runId, @message, @isError, @createdAt)",
            new
            {
                runId = runOutput.TaskRunId,
                message = runOutput.Message,
                isError = Convert.ToInt32(runOutput.IsError),
                createdAt = runOutput.CreatedAt
            });
    }

    public async Task AddScriptRunOutputAsync(List<PikaScriptRunOutput> runOutputs)
    {
        if (runOutputs == null || !runOutputs.Any())
        {
            return;
        }

        List<dynamic> objs = [];
        foreach (var item in runOutputs)
        {
            objs.Add(new
            {
                runId = item.TaskRunId,
                message = item.Message,
                isError = Convert.ToInt32(item.IsError),
                createdAt = item.CreatedAt
            });
        }

        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        _ = await connection.ExecuteAsync(
            "INSERT INTO script_run_output(run_id, message, is_error, created_at) VALUES(@runId, @message, @isError, @createdAt)",
            objs, transaction: transaction);

        await transaction.CommitAsync();
    }

    public async Task<List<PikaScriptRunOutput>> GetScriptRunOutputs(long scriptRunId, long laterThan = default,
        int limit = -1, bool desc = true)
    {
        await using SqliteConnection connection = new(_connectionString);
        var createdAtClause = laterThan == default ? "" : "AND created_at>@laterThan";
        var limitClause = limit > 0 ? $"LIMIT {limit} OFFSET 0" : "";
        var orderClause = desc ? "DESC" : "ASC";
        var result = await connection.QueryAsync<PikaScriptRunOutput>(
            $"SELECT * FROM script_run_output WHERE run_id=@runId {createdAtClause} ORDER BY id {orderClause} {limitClause}",
            new
            {
                runId = scriptRunId,
                laterThan
            });

        return result.AsList();
    }

    public async Task UpdateScriptRunStatusAsync(long runId, PikaScriptStatus status)
    {
        await using SqliteConnection connection = new(_connectionString);
        var sql = status is PikaScriptStatus.Completed or PikaScriptStatus.Dead or PikaScriptStatus.Stopped
            ? "UPDATE script_run SET status=@status, completed_at=@time WHERE id=@id"
            : status == PikaScriptStatus.Running
                ? "UPDATE script_run SET status=@status, started_at=@time WHERE id=@id"
                : "UPDATE script_run SET status=@status WHERE id=@id";
        _ = await connection.ExecuteAsync(sql, new { id = runId, status = (int)status, time = DateTime.Now.Ticks });
    }

    public async Task<PikaScriptRun> GetScriptRunAsync(long runId)
    {
        await using SqliteConnection connection = new(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<PikaScriptRun>("SELECT * FROM script_run WHERE id=@id",
            new { id = runId });
    }

    #endregion

    #region Setting

    public async Task<string> GetSetting(string key)
    {
        await using SqliteConnection connection = new(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<string>("SELECT value FROM setting WHERE key=@key",
            new { key });
    }

    public async Task InsertOrUpdateSetting(string key, string value)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "INSERT OR REPLACE INTO setting(key, value, last_modified_at) VALUES (@key, @value, @time)",
            new { key, value, time = DateTime.Now.Ticks });
    }

    #endregion

    #region Drive

    public async Task AddOrUpdatePikaDrivesAsync(List<PikaDriveTable> drives)
    {
        await using SqliteConnection connection = new(_connectionString);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM drive_partition;" +
            "DELETE FROM drive;",
            transaction: transaction);

        foreach (var drive in drives)
        {
            await connection.ExecuteAsync(
                "INSERT INTO drive(id, name, path, size, type, model, serial, tran, created_at) " +
                "VALUES(@id, @name, @path, @size, @type, @model, @serial, @tran, @createdAt)",
                new
                {
                    id = drive.Id,
                    name = drive.Name,
                    path = drive.Path,
                    size = drive.Size,
                    type = drive.Type,
                    model = drive.Model,
                    serial = drive.Serial,
                    tran = drive.Tran,
                    createdAt = DateTime.Now.Ticks,
                    lastUpdatedAt = DateTime.Now.Ticks
                }, transaction);
            foreach (var parition in drive.Partitions)
            {
                await connection.ExecuteAsync(
                    "INSERT INTO drive_partition (drive_id, name, path, size, type, mount_point, created_at, uuid, label) " +
                    "VALUES (@driveId, @name, @path, @size, @type, @mountPoint, @createdAt, @uuid, @label)",
                    new
                    {
                        driveId = drive.Id,
                        name = parition.Name,
                        path = parition.Path,
                        size = parition.Size,
                        type = parition.Type,
                        mountPoint = parition.MountPoint,
                        createdAt = DateTime.Now.Ticks,
                        lastUpdatedAt = DateTime.Now.Ticks,
                        uuid = parition.Uuid,
                        label = parition.Label
                    }, transaction);
            }
        }

        await transaction.CommitAsync();
    }

    public async Task<List<PikaDriveTable>> GetPikaDrivesAsync()
    {
        await using SqliteConnection connection = new(_connectionString);
        var result = (await connection.QueryAsync<PikaDriveTable>("SELECT * FROM drive ORDER BY name")).AsList();
        if (!result.Any())
        {
            return result;
        }

        var partitions = (await connection.QueryAsync<PikaDrivePartitionTable>("SELECT * FROM drive_partition")).AsList();
        foreach (var item in result)
        {
            item.Partitions.AddRange(partitions.Where(x => x.DriveId == item.Id).OrderBy(x => x.Name));
        }

        return result;
    }

    public async Task AddPikaDriveSmartctlAsync(PikaDriveSmartctlTable pikaDriveSmartctl)
    {
        if (pikaDriveSmartctl == null) return;

        await using SqliteConnection connection = new(_connectionString);

        var sql = @"INSERT INTO drive_smartctl(
            drive_id, passed, reallocated_sector_ct, current_pending_sector, offline_uncorrectable, reported_uncorrect,
            start_stop_count, power_on_hours, created_at, power_cycle_count, command_timeout, power_off_restart_count, load_cycle_count, udma_crc_error_count)
            VALUES(@DriveId, @passed, @ReallocatedSectorCt, @CurrentPendingSector, @OfflineUncorrectable, @ReportedUncorrect,
            @StartStopCount, @PowerOnHours, @CreatedAt, @PowerCycleCount, @CommandTimeout, @PowerOffRestartCount, @LoadCycleCount, @UdmaCrcErrorCount)";

        var dp = new DynamicParameters();
        dp.Add("DriveId", pikaDriveSmartctl.DriveId);
        dp.Add("passed", pikaDriveSmartctl.Passed ? 1 : 0);
        dp.Add("ReallocatedSectorCt", pikaDriveSmartctl.ReallocatedSectorCt);
        dp.Add("CurrentPendingSector", pikaDriveSmartctl.CurrentPendingSector);
        dp.Add("OfflineUncorrectable", pikaDriveSmartctl.OfflineUncorrectable);
        dp.Add("ReportedUncorrect", pikaDriveSmartctl.ReportedUncorrect);
        dp.Add("StartStopCount", pikaDriveSmartctl.StartStopCount);
        dp.Add("PowerOnHours", pikaDriveSmartctl.PowerOnHours);
        dp.Add("CreatedAt", DateTime.Now.Ticks);
        dp.Add("PowerCycleCount", pikaDriveSmartctl.PowerCycleCount);
        dp.Add("CommandTimeout", pikaDriveSmartctl.CommandTimeout);
        dp.Add("PowerOffRestartCount", pikaDriveSmartctl.PowerOffRestartCount);
        dp.Add("LoadCycleCount", pikaDriveSmartctl.LoadCycleCount);
        dp.Add("UdmaCrcErrorCount", pikaDriveSmartctl.UdmaCrcErrorCount);

        await connection.ExecuteAsync(sql, dp);
    }

    public async Task<PikaDriveSmartctlTable> GetPikaDriveSmartTableAsync(string driveId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.QueryFirstOrDefaultAsync<PikaDriveSmartctlTable>(
                "SELECT * FROM drive_smartctl WHERE drive_id = @driveId ORDER BY created_at DESC LIMIT 1",
                new { driveId });
        return result;
    }

    public async Task<List<PikaDriveSmartctlTable>> GetPikaDriveSmartctlTablesAsync(string driveId, DateTime from)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.QueryAsync<PikaDriveSmartctlTable>(
                "SELECT * FROM drive_smartctl WHERE drive_id = @driveId AND created_at >= @from ORDER BY created_at DESC",
                new { driveId, from = from.Ticks });
        return result.AsList();
    }

    #endregion

    #region Misc

    public long GetDbSize()
    {
        return new FileInfo(_pikaDb).Length;
    }

    public async Task VacuumDbAsync()
    {
        var retainDbSize = Convert.ToInt32(await GetSetting(PikaSettingKey.RetainSizeInMb)) * 1024 * 1024;
        await using SqliteConnection connection = new(_connectionString);

        // Cleanup dead script runs
        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM script_run WHERE status = {(int)PikaScriptStatus.Dead} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM script_run_output WHERE run_id=@runId; DELETE FROM script_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }

        // Cleanup stopped script runs
        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM script_run WHERE status = {(int)PikaScriptStatus.Stopped} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM script_run_output WHERE run_id=@runId; DELETE FROM script_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }

        // Cleanup completed script runs 
        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM script_run WHERE status = {(int)PikaScriptStatus.Completed} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM script_run_output WHERE run_id=@runId; DELETE FROM script_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }
    }

    #endregion
}