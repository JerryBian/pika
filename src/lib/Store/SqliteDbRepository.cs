using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pika.Lib.Dapper;
using Pika.Lib.Model;

namespace Pika.Lib.Store;

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

        Directory.CreateDirectory(_options.DbLocation);
        _pikaDb = Path.Combine(_options.DbLocation, "pika.db");
        if (File.Exists(_pikaDb) && _options.ForceRecreateDb)
        {
            File.Delete(_pikaDb);
        }

        var connectionStringBuilder = new SqliteConnectionStringBuilder
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

        var startupFile = Path.Combine(baseDir, "Store", "startup.sql");
        if (!File.Exists(startupFile))
        {
            throw new Exception($"Cannot find startup script: {startupFile}");
        }

        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = await File.ReadAllTextAsync(startupFile, Encoding.UTF8);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<long> AddTaskAsync(PikaTask task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO task(name, script, description, shell_name, shell_option, shell_ext, is_temp) VALUES(@name, @script, @desc, @shellName, @shellOption, @shellExt, @isTemp) RETURNING id",
            new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description,
                shellName = task.ShellName,
                shellOption = task.ShellOption,
                shellExt = task.ShellExt,
                isTemp = task.IsTemp
            });
        if (id == null)
        {
            return -1;
        }

        return Convert.ToInt64(id);
    }

    public async Task UpdateTaskAsync(PikaTask task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "UPDATE task SET name=@name, script=@script, description=@desc, shell_name=@shellName, shell_option=@shellOption, shell_ext=@shellExt, last_modified_at=DATETIME('now', 'localtime') WHERE id=@id",
            new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description,
                id = task.Id,
                shellName = task.ShellName,
                shellOption = task.ShellOption,
                shellExt = task.ShellExt
            });
    }

    public async Task<int> GetRunsCountAsync(string whereClause = "")
    {
        await using var connection = new SqliteConnection(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        var result =
            await connection.ExecuteScalarAsync<int>($"SELECT COUNT(1) FROM task_run {whereClause}");
        return result;
    }

    public async Task<int> GetRunsCountAsync(long taskId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM task_run WHERE task_id=@taskId",
                new {taskId});
        return result;
    }

    public long GetDbSize()
    {
        return new FileInfo(_pikaDb).Length;
    }

    public async Task VacuumDbAsync()
    {
        var retainDbSize = Convert.ToInt32(await GetSetting(SettingKey.RetainSizeInMb)) * 1024 * 1024;
        await using var connection = new SqliteConnection(_connectionString);

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int) PikaTaskStatus.Dead} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            await connection.ExecuteAsync("VACUUM");
        }

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int) PikaTaskStatus.Stopped} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            await connection.ExecuteAsync("VACUUM");
        }

        while (GetDbSize() > retainDbSize)
        {
            var runId = await connection.QueryFirstOrDefaultAsync<int>(
                $"SELECT id FROM task_run WHERE status = {(int) PikaTaskStatus.Completed} ORDER BY created_at ASC LIMIT 1");
            if (runId == default)
            {
                break;
            }

            await connection.ExecuteAsync(
                "DELETE FROM task_run_output WHERE run_id=@runId; DELETE FROM task_run WHERE id=@runId",
                new
                {
                    runId
                });
            await connection.ExecuteAsync("VACUUM");
        }
    }

    public async Task<int> GetTasksCountAsync()
    {
        await using var connection = new SqliteConnection(_connectionString);
        var result =
            await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM task WHERE is_temp = 0");
        return result;
    }

    public async Task<PikaTask> GetTaskAsync(long taskId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var result =
            await connection.QuerySingleOrDefaultAsync<PikaTask>("SELECT * FROM task WHERE id=@id", new {id = taskId});
        return result;
    }

    public async Task DeleteTaskAsync(long taskId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
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
        await using var connection = new SqliteConnection(_connectionString);
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
        await using var connection = new SqliteConnection(_connectionString);
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
        var status = new PikaSystemStatus();
        await using var connection = new SqliteConnection(_connectionString);
        var sql = "SELECT COUNT(*) FROM task WHERE is_temp = 0;" +
                  "SELECT COUNT(*) FROM task_run;" +
                  $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Pending}';" +
                  $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Running}';" +
                  $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Completed}';" +
                  $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Stopped}';" +
                  $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Dead}';" +
                  "SELECT a.id, a.name, count(1) run_count FROM task a JOIN task_run b ON a.id = b.task_id WHERE a.is_temp = 0 GROUP BY a.id ORDER BY COUNT(1) DESC, a.created_at ASC LIMIT 8 OFFSET 0;" +
                  $"SELECT * FROM task_run WHERE status='{(int) PikaTaskStatus.Completed}' ORDER BY (julianday(IFNULL(completed_at, DATETIME('now', 'localtime'))) - julianday(created_at)) DESC LIMIT 8 OFFSET 0";
        using (var multi = await connection.QueryMultipleAsync(sql))
        {
            status.TaskCount = await multi.ReadSingleAsync<int>();
            status.RunCount = await multi.ReadSingleAsync<int>();
            status.TaskInPendingCount = await multi.ReadSingleAsync<int>();
            status.TaskInRunningCount = await multi.ReadSingleAsync<int>();
            status.TaskInCompletedCount = await multi.ReadSingleAsync<int>();
            status.TaskInStoppedCount = await multi.ReadSingleAsync<int>();
            status.TaskInDeadCount = await multi.ReadSingleAsync<int>();
            var mostRunTasks = await multi.ReadAsync<dynamic>();
            foreach (var mostRunTask in mostRunTasks)
            {
                var task = new PikaTask
                {
                    Id = Convert.ToInt64(mostRunTask.id),
                    Name = Convert.ToString(mostRunTask.name)
                };
                status.MostRunTasks.Add(new KeyValuePair<PikaTask, int>(task, Convert.ToInt32(mostRunTask.run_count)));
            }

            status.LongestRuns.AddRange(await multi.ReadAsync<PikaTaskRun>());
        }

        status.DbSize = GetDbSize();
        return status;
    }

    public async Task<long> AddTaskRunAsync(PikaTaskRun run)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO task_run(task_id, status, script, shell_name, shell_option, shell_ext) VALUES(@taskId, @status, @script, @shellName, @shellOption, @shellExt) RETURNING id",
            new
            {
                taskId = run.TaskId,
                status = (int) run.Status,
                script = run.Script,
                shellName = run.ShellName,
                shellOption = run.ShellOption,
                shellExt = run.ShellExt
            });
        if (id == null)
        {
            return -1;
        }

        return Convert.ToInt64(id);
    }

    public async Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "INSERT INTO task_run_output(run_id, message, is_error) VALUES(@runId, @message, @isError)", new
            {
                runId = runOutput.TaskRunId,
                message = runOutput.Message,
                isError = Convert.ToInt32(runOutput.IsError)
            });
    }

    public async Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, DateTime laterThan = default,
        int limit = -1)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var createdAtClause = laterThan == default ? "" : "AND julianday(created_at)>julianday(@laterThan)";
        var limitClause = limit > 0 ? $"LIMIT {limit} OFFSET 0" : "";
        var result = await connection.QueryAsync<PikaTaskRunOutput>(
            $"SELECT * FROM task_run_output WHERE run_id=@runId {createdAtClause} ORDER BY id DESC {limitClause}",
            new
            {
                runId = taskRunId,
                laterThan
            });

        return result.AsList();
    }

    public async Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status)
    {
        await using var connection = new SqliteConnection(_connectionString);
        string sql;
        if (status == PikaTaskStatus.Completed || status == PikaTaskStatus.Dead || status == PikaTaskStatus.Stopped)
        {
            sql =
                "UPDATE task_run SET status=@status, completed_at=DATETIME('now', 'localtime') WHERE id=@id";
        }
        else if (status == PikaTaskStatus.Running)
        {
            sql = "UPDATE task_run SET status=@status, started_at=DATETIME('now', 'localtime') WHERE id=@id";
        }
        else
        {
            sql = "UPDATE task_run SET status=@status WHERE id=@id";
        }

        await connection.ExecuteAsync(sql, new {id = runId, status = (int) status});
    }

    public async Task<PikaTaskRun> GetTaskRunAsync(long runId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<PikaTaskRun>("SELECT * FROM task_run WHERE id=@id",
            new {id = runId});
    }

    public async Task<string> GetSetting(string key)
    {
        await using var connection = new SqliteConnection(_connectionString);
        return await connection.QuerySingleOrDefaultAsync<string>("SELECT value FROM setting WHERE key=@key",
            new {key});
    }

    public async Task InsertOrUpdateSetting(string key, string value)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.ExecuteAsync(
            "INSERT OR REPLACE INTO setting(key, value, last_modified_at) VALUES (@key, @value, DATETIME('now', 'localtime'))",
            new {key, value});
    }
}