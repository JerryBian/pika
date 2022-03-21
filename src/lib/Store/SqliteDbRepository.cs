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
            "INSERT INTO task(name, script, description) VALUES(@name, @script, @desc) RETURNING id", new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description
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
        await connection.ExecuteAsync("UPDATE task SET name=@name, script=@script, description=@desc WHERE id=@id",
            new
            {
                name = task.Name,
                script = task.Script,
                desc = task.Description,
                id = task.Id
            });
    }

    public async Task<PikaTask> GetTaskAsync(long taskId)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var result =
            await connection.QuerySingleOrDefaultAsync<PikaTask>("SELECT * FROM task WHERE id=@id", new {id = taskId});
        return result;
    }

    public async Task<List<PikaTask>> GetTasksAsync(int limit, int offset, string whereClause = "",
        string orderByClause = "")
    {
        await using var connection = new SqliteConnection(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        var sql = $"SELECT * FROM task {whereClause} {orderByClause} LIMIT {limit} OFFSET {offset}";
        return (await connection.QueryAsync<PikaTask>(sql)).AsList();
    }

    public async Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit, int offset, string whereClause = "",
        string orderByClause = "")
    {
        await using var connection = new SqliteConnection(_connectionString);
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        var sql = $"SELECT * FROM task_run {whereClause} {orderByClause} LIMIT {limit} OFFSET {offset}";
        return (await connection.QueryAsync<PikaTaskRun>(sql)).AsList();
    }

    public async Task<PikaSystemStatus> GetSystemStatusAsync()
    {
        var status = new PikaSystemStatus();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM task;" +
                              "SELECT COUNT(*) FROM task_run;" +
                              $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Pending}';" +
                              $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Running}';" +
                              $"SELECT COUNT(*) FROM task_run WHERE status='{(int) PikaTaskStatus.Completed}';" +
                              "SELECT a.* FROM task a JOIN task_run b ON a.id = b.task_id GROUP BY a.id ORDER BY COUNT(1) DESC LIMIT 8 OFFSET 0;" +
                              $"SELECT * FROM task_run WHERE status='{(int) PikaTaskStatus.Completed}' ORDER BY (julianday(IFNULL(completed_at, DATETIME('now', 'localtime'))) - julianday(created_at)) DESC LIMIT 8 OFFSET 0";
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            status.TaskCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            status.RunCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            status.TaskInPendingCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            status.TaskInRunningCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            status.TaskInCompletedCount = reader.GetInt32(0);
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var desc = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var script = reader.GetString(3);
            var isFavorite = reader.GetBoolean(4);
            var createdAt = reader.GetDateTime(5);
            status.MostRunTasks.Add(new PikaTask
            {
                CreatedAt = createdAt,
                Description = desc,
                Id = id,
                IsFavorite = isFavorite,
                Name = name,
                Script = script
            });
        }

        await reader.NextResultAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var taskId = reader.GetInt64(1);
            var s = reader.GetInt64(2);
            var script = reader.GetString(3);
            var createdAt = reader.GetDateTime(4);
            var completedAt = reader.IsDBNull(5) ? default : reader.GetDateTime(5);
            status.LongestRuns.Add(new PikaTaskRun
            {
                CreatedAt = createdAt,
                TaskId = taskId,
                Id = id,
                Status = (PikaTaskStatus) s,
                CompletedAt = completedAt,
                Script = script
            });
        }

        status.DbSize = new FileInfo(_pikaDb).Length;
        return status;
    }

    public async Task<long> AddTaskRunAsync(long taskId)
    {
        var task = await GetTaskAsync(taskId);
        await using var connection = new SqliteConnection(_connectionString);
        var id = await connection.ExecuteScalarAsync(
            "INSERT INTO task_run(task_id, status, script) VALUES(@taskId, @status, @script) RETURNING id", new
            {
                taskId,
                script = task.Script,
                status = (int) PikaTaskStatus.Pending
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

    public async Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, DateTime laterThan)
    {
        await using var connection = new SqliteConnection(_connectionString);
        var result = await connection.QueryAsync<PikaTaskRunOutput>(
            "SELECT * FROM task_run_output WHERE run_id=@runId ORDER BY id DESC LIMIT 100 OFFSET 0",
            new
            {
                runId = taskRunId
            });

        return result.AsList();
    }

    public async Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status)
    {
        await using var connection = new SqliteConnection(_connectionString);
        string sql;
        if (status == PikaTaskStatus.Completed || status == PikaTaskStatus.Dead)
        {
            sql =
                "UPDATE task_run SET status=@status, completed_at=DATETIME('now', 'localtime') WHERE id=@id";
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