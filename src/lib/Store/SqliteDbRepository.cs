﻿using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pika.Lib.Dapper;
using Pika.Lib.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        var startupFile = Path.Combine(baseDir, "Store", "startup.sql");
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
        var sql =  "SELECT COUNT(*) FROM task_run;" +
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
}