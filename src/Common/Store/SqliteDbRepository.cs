using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pika.Common.App;
using Pika.Common.Dapper;
using Pika.Common.Drive;
using Pika.Common.Model;
using System.Text;

namespace Pika.Common.Store;

public class SqliteDbRepository : IDbRepository
{
    private readonly PikaOptions _options;
    private string _connectionString;
    private string _pikaDb;

    public SqliteDbRepository(IOptions<PikaOptions> options)
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
            DefaultTimeout = 30,
            Pooling = true,
            ForeignKeys = true
        };
        _connectionString = connectionStringBuilder.ToString();
        var baseDir = AppContext.BaseDirectory;
        if (string.IsNullOrEmpty(baseDir))
        {
            throw new Exception("Cannot find base dir.");
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

    public async Task<long> AddTaskAsync(PikaTask task)
    {
        await using SqliteConnection connection = new(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO task(name, script, description, shell_name, shell_option, shell_ext, is_temp, created_at, last_modified_at) " +
            "VALUES(@name, @script, @desc, @shellName, @shellOption, @shellExt, @isTemp, @createdAt, @lastModifiedAt) RETURNING id",
            new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description,
                shellName = task.ShellName,
                shellOption = task.ShellOption,
                shellExt = task.ShellExt,
                isTemp = task.IsTemp,
                createdAt = task.CreatedAt,
                lastModifiedAt = task.LastModifiedAt
            });
        return id == null ? -1 : Convert.ToInt64(id);
    }

    public async Task UpdateTaskAsync(PikaTask task)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "UPDATE task SET name=@name, script=@script, description=@desc, shell_name=@shellName, shell_option=@shellOption, shell_ext=@shellExt, last_modified_at=@lastModifiedAt WHERE id=@id",
            new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description,
                id = task.Id,
                shellName = task.ShellName,
                shellOption = task.ShellOption,
                shellExt = task.ShellExt,
                lastModifiedAt = task.LastModifiedAt
            });
    }

    public async Task<int> GetRunsCountAsync(string whereClause = "")
    {
        await using SqliteConnection connection = new(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        var result =
            await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM task_run {whereClause}");
        return result;
    }

    public async Task<int> GetRunsCountAsync(long taskId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM task_run WHERE task_id=@taskId",
                new { taskId });
        return result;
    }

    public long GetDbSize()
    {
        return new FileInfo(_pikaDb).Length;
    }

    public async Task VacuumDbAsync()
    {
        var retainDbSize = Convert.ToInt32(await GetSetting(SettingKey.RetainSizeInMb)) * 1024 * 1024;
        await using SqliteConnection connection = new(_connectionString);

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int)PikaTaskStatus.Dead} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int)PikaTaskStatus.Stopped} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int)PikaTaskStatus.Completed} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            _ = await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            _ = await connection.ExecuteAsync("VACUUM");
        }
    }

    public async Task<int> GetTasksCountAsync()
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM task WHERE is_temp = 0");
        return result;
    }

    public async Task<PikaTask> GetTaskAsync(long taskId)
    {
        await using SqliteConnection connection = new(_connectionString);
        var result =
            await connection.QuerySingleOrDefaultAsync<PikaTask>("SELECT * FROM task WHERE id=@id", new { id = taskId });
        return result;
    }

    public async Task DeleteTaskAsync(long taskId)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "DELETE FROM task_run_output WHERE run_id IN(SELECT id FROM task_run WHERE task_id=@taskId);" +
            "DELETE FROM task_run WHERE task_id=@taskId;" +
            "DELETE FROM task WHERE id=@taskId", new
            {
                taskId
            });
    }

    public async Task<List<PikaTask>> GetTasksAsync(int limit = 0, int offset = -1, string whereClause = "",
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

        var sql = $"SELECT * FROM task WHERE is_temp = 0 {whereClause} {orderByClause} {limitClause}";
        return (await connection.QueryAsync<PikaTask>(sql)).AsList();
    }

    public async Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit = 0, int offset = -1, string whereClause = "",
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

        var sql = $"SELECT * FROM task_run {whereClause} {orderByClause} {limitClause}";
        return (await connection.QueryAsync<PikaTaskRun>(sql)).AsList();
    }

    public async Task<PikaSystemStatus> GetSystemStatusAsync()
    {
        PikaSystemStatus status = new();
        await using SqliteConnection connection = new(_connectionString);
        var sql = "SELECT COUNT(*) FROM task_run;" +
                  "SELECT a.*, (SELECT COUNT(1) FROM task_run WHERE task_id = a.id) AS RunCount, (SELECT MAX(created_at) FROM task_run WHERE task_id = a.id) AS LastRun FROM task a WHERE a.is_temp = 0 ORDER BY LastRun DESC;" +
                  "SELECT a.*, b.name AS TaskName FROM task_run a JOIN task b ON a.task_id = b.id ORDER BY created_at DESC LIMIT 10 OFFSET 0;";
        using (var multi = await connection.QueryMultipleAsync(sql))
        {
            status.RunCount = await multi.ReadSingleAsync<int>();
            status.SavedTasks.AddRange(await multi.ReadAsync<PikaTask>());

            status.LatestRuns.AddRange(await multi.ReadAsync<PikaTaskRun>());
        }

        status.DbSize = GetDbSize();
        return status;
    }

    public async Task<long> AddTaskRunAsync(PikaTaskRun run)
    {
        await using SqliteConnection connection = new(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO task_run(task_id, status, script, shell_name, shell_option, shell_ext, created_at, started_at, completed_at) " +
            "VALUES(@taskId, @status, @script, @shellName, @shellOption, @shellExt, @createdAt, 0, 0) RETURNING id",
            new
            {
                taskId = run.TaskId,
                status = (int)run.Status,
                script = run.Script,
                shellName = run.ShellName,
                shellOption = run.ShellOption,
                shellExt = run.ShellExt,
                createdAt = run.CreatedAt
            });
        return id == null ? -1 : Convert.ToInt64(id);
    }

    public async Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput)
    {
        await using SqliteConnection connection = new(_connectionString);
        _ = await connection.ExecuteAsync(
            "INSERT INTO task_run_output(run_id, message, is_error, created_at) VALUES(@runId, @message, @isError, @createdAt)",
            new
            {
                runId = runOutput.TaskRunId,
                message = runOutput.Message,
                isError = Convert.ToInt32(runOutput.IsError),
                createdAt = runOutput.CreatedAt
            });
    }

    public async Task AddTaskRunOutputAsync(List<PikaTaskRunOutput> runOutputs)
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
            "INSERT INTO task_run_output(run_id, message, is_error, created_at) VALUES(@runId, @message, @isError, @createdAt)",
            objs, transaction: transaction);

        await transaction.CommitAsync();
    }

    public async Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, long laterThan = default,
        int limit = -1, bool desc = true)
    {
        await using SqliteConnection connection = new(_connectionString);
        var createdAtClause = laterThan == default ? "" : "AND created_at>@laterThan";
        var limitClause = limit > 0 ? $"LIMIT {limit} OFFSET 0" : "";
        var orderClause = desc ? "DESC" : "ASC";
        var result = await connection.QueryAsync<PikaTaskRunOutput>(
            $"SELECT * FROM task_run_output WHERE run_id=@runId {createdAtClause} ORDER BY id {orderClause} {limitClause}",
            new
            {
                runId = taskRunId,
                laterThan
            });

        return result.AsList();
    }

    public async Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status)
    {
        await using SqliteConnection connection = new(_connectionString);
        var sql = status is PikaTaskStatus.Completed or PikaTaskStatus.Dead or PikaTaskStatus.Stopped
            ? "UPDATE task_run SET status=@status, completed_at=@time WHERE id=@id"
            : status == PikaTaskStatus.Running
                ? "UPDATE task_run SET status=@status, started_at=@time WHERE id=@id"
                : "UPDATE task_run SET status=@status WHERE id=@id";
        _ = await connection.ExecuteAsync(sql, new { id = runId, status = (int)status, time = DateTime.Now.Ticks });
    }

    public async Task<PikaTaskRun> GetTaskRunAsync(long runId)
    {
        await using SqliteConnection connection = new(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<PikaTaskRun>("SELECT * FROM task_run WHERE id=@id",
            new { id = runId });
    }

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

    #region PikaDrive

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
        if(!result.Any())
        {
            return result;
        }

        var partitions = (await connection.QueryAsync<PikaDrivePartitionTable>("SELECT * FROM drive_partition")).AsList();
        foreach(var item in result)
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
}