using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using Pika.Lib.Model;

namespace Pika.Lib.Store;

public class SqliteDbRepository : IDbRepository
{
    private readonly PikaOptions _options;
    private string _connectionString;

    public SqliteDbRepository(IOptions<PikaOptions> options)
    {
        _options = options.Value;
    }

    public async Task StartupAsync()
    {
        if (string.IsNullOrEmpty(_options.DbLocation))
        {
            throw new Exception("Please specify the DB location.");
        }

        Directory.CreateDirectory(_options.DbLocation);
        var dbFile = Path.Combine(_options.DbLocation, "pika.db");
        if (File.Exists(dbFile) && _options.ForceRecreateDb)
        {
            File.Delete(dbFile);
        }

        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            Cache = SqliteCacheMode.Shared,
            DataSource = dbFile,
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
        await using var command = connection.CreateCommand();
        command.CommandText = "INSERT INTO task(name, script, description) VALUES(@name, @script, @desc) RETURNING id";
        command.Parameters.AddWithValue("@name", task.Name);
        command.Parameters.AddWithValue("@script", task.Script);
        command.Parameters.AddWithValue("@desc", task.Description);
        await connection.OpenAsync();
        var id = await command.ExecuteScalarAsync();
        if (id == null)
        {
            return -1;
        }

        return Convert.ToInt64(id);
    }

    public async Task UpdateTaskAsync(PikaTask task)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE task SET name=@name, script=@script, description=@desc WHERE id=@id";
        command.Parameters.AddWithValue("@name", task.Name);
        command.Parameters.AddWithValue("@script", task.Script);
        command.Parameters.AddWithValue("@desc", task.Description);
        command.Parameters.AddWithValue("@id", task.Id);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<PikaTask> GetTaskAsync(long taskId)
    {
        var result = new List<PikaTask>();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM task WHERE id=@id";
        command.Parameters.AddWithValue("@id", taskId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var desc = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var script = reader.GetString(3);
            var isFavorite = reader.GetBoolean(4);
            var createdAt = reader.GetDateTime(5);
            result.Add(new PikaTask
            {
                CreatedAt = createdAt,
                Description = desc,
                Id = id,
                IsFavorite = isFavorite,
                Name = name,
                Script = script
            });
        }

        return result.FirstOrDefault();
    }

    public async Task<List<PikaTask>> GetTasksAsync(int limit, int offset, string whereClause = "",
        string orderByClause = "")
    {
        var result = new List<PikaTask>();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        command.CommandText = $"SELECT * FROM task {whereClause} {orderByClause} LIMIT {limit} OFFSET {offset}";
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
            var desc = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
            var script = reader.GetString(3);
            var isFavorite = reader.GetBoolean(4);
            var createdAt = reader.GetDateTime(5);
            result.Add(new PikaTask
            {
                CreatedAt = createdAt,
                Description = desc,
                Id = id,
                IsFavorite = isFavorite,
                Name = name,
                Script = script
            });
        }

        return result;
    }

    public async Task<List<PikaTaskRun>> GetTaskRunsAsync(int limit, int offset, string whereClause = "",
        string orderByClause = "")
    {
        var result = new List<PikaTaskRun>();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        whereClause = string.IsNullOrEmpty(whereClause) ? string.Empty : $"WHERE {whereClause}";
        orderByClause = string.IsNullOrEmpty(orderByClause) ? string.Empty : $"ORDER BY {orderByClause}";
        command.CommandText = $"SELECT * FROM task_run {whereClause} {orderByClause} LIMIT {limit} OFFSET {offset}";
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var taskId = reader.GetInt64(1);
            var status = reader.GetInt64(2);
            var script = reader.GetString(3);
            var createdAt = reader.GetDateTime(4);
            var completedAt = reader.IsDBNull(5) ? default : reader.GetDateTime(5);
            result.Add(new PikaTaskRun
            {
                CreatedAt = createdAt,
                TaskId = taskId,
                Id = id,
                Status = (PikaTaskStatus) status,
                CompletedAt = completedAt,
                Script = script
            });
        }

        return result;
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
                              $"SELECT COUNT(*) FROM task_run WHERE status='{(int)PikaTaskStatus.Completed}';" +
                              "SELECT a.* FROM task a JOIN task_run b ON a.id = b.task_id GROUP BY a.id ORDER BY COUNT(1) DESC LIMIT 8 OFFSET 0;" +
                              $"SELECT * FROM task_run WHERE status='{(int)PikaTaskStatus.Completed}' ORDER BY (julianday(IFNULL(completed_at, DATETIME('now', 'localtime'))) - julianday(created_at)) DESC LIMIT 8 OFFSET 0";
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
                Status = (PikaTaskStatus)s,
                CompletedAt = completedAt,
                Script = script
            });
        }

            return status;
    }

    public async Task<long> AddTaskRunAsync(long taskId)
    {
        var task = await GetTaskAsync(taskId);
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO task_run(task_id, status, script) VALUES(@taskId, @status, @script) RETURNING id";
        command.Parameters.AddWithValue("@taskId", taskId);
        command.Parameters.AddWithValue("@script", task.Script);
        command.Parameters.AddWithValue("@status", (int) PikaTaskStatus.Pending);
        await connection.OpenAsync();
        var id = await command.ExecuteScalarAsync();
        if (id == null)
        {
            return -1;
        }

        return Convert.ToInt64(id);
    }

    public async Task AddTaskRunOutputAsync(PikaTaskRunOutput runOutput)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO task_run_output(run_id, message, is_error) VALUES(@runId, @message, @isError)";
        command.Parameters.AddWithValue("@runId", runOutput.TaskRunId);
        command.Parameters.AddWithValue("@message", runOutput.Message);
        command.Parameters.AddWithValue("@isError", Convert.ToInt32(runOutput.IsError));
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<PikaTaskRunOutput>> GetTaskRunOutputs(long taskRunId, DateTime laterThan)
    {
        var result = new List<PikaTaskRunOutput>();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT * FROM task_run_output WHERE run_id=@runId AND julianday(created_at)>julianday(@laterThan) ORDER BY created_at ASC";
        command.Parameters.AddWithValue("@runId", taskRunId);
        command.Parameters.AddWithValue("@laterThan", laterThan);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var message = reader.GetString(2);
            var isError = reader.GetBoolean(3);
            var createdAt = reader.GetDateTime(4);
            result.Add(new PikaTaskRunOutput
            {
                CreatedAt = createdAt,
                Id = id,
                IsError = isError,
                Message = message,
                TaskRunId = taskRunId
            });
        }

        return result;
    }

    public async Task UpdateTaskRunStatusAsync(long runId, PikaTaskStatus status)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        if (status == PikaTaskStatus.Completed || status == PikaTaskStatus.Dead)
        {
            command.CommandText =
                "UPDATE task_run SET status=@status, completed_at=DATETIME('now', 'localtime') WHERE id=@id";
        }
        else
        {
            command.CommandText = "UPDATE task_run SET status=@status WHERE id=@id";
        }

        command.Parameters.AddWithValue("@status", (int) status);
        command.Parameters.AddWithValue("@id", runId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<PikaTaskRun> GetTaskRunAsync(long runId)
    {
        var result = new List<PikaTaskRun>();
        await using var connection = new SqliteConnection(_connectionString);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM task_run WHERE id=@id";
        command.Parameters.AddWithValue("@id", runId);
        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var id = reader.GetInt64(0);
            var taskId = reader.GetInt64(1);
            var status = reader.GetInt64(2);
            var script = reader.GetString(3);
            var createdAt = reader.GetDateTime(4);
            var completedAt = reader.IsDBNull(5) ? default : reader.GetDateTime(5);
            result.Add(new PikaTaskRun
            {
                CreatedAt = createdAt,
                TaskId = taskId,
                Id = id,
                Status = (PikaTaskStatus) status,
                CompletedAt = completedAt,
                Script = script
            });
        }

        return result.FirstOrDefault();
    }
}