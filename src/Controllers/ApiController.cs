using ExecDotnet;
using Microsoft.AspNetCore.Mvc;
using Pika.Common.Command;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pika.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly ICommandManager _commandManager;
    private readonly IDbRepository _repository;
    private readonly PikaSetting _setting;

    public ApiController(PikaSetting setting, IDbRepository repository, ICommandManager commandManager)
    {
        _setting = setting;
        _repository = repository;
        _commandManager = commandManager;
    }

    [HttpDelete("app/{id}")]
    public async Task<ApiResponse<object>> DeleteAppAsync([FromRoute] long id)
    {
        ApiResponse<object> response = new();
        try
        {
            await _repository.DeleteAppAsync(id);
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPut("app/add")]
    public async Task<ApiResponse<object>> AddAppAsync([FromForm] PikaApp app)
    {
        ApiResponse<object> response = new();
        try
        {
            app.CreatedAt = app.LastModifiedAt = DateTime.Now.Ticks;
            var id = await _repository.AddAppAsync(app);
            response.RedirectTo = $"/app/{id}";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("app/update")]
    public async Task<ApiResponse<object>> UpdateAppAsync([FromForm] PikaApp app)
    {
        ApiResponse<object> response = new();
        try
        {
            app.LastModifiedAt = DateTime.Now.Ticks;
            await _repository.UpdateAppAsync(app);
            response.RedirectTo = $"/app/{app.Id}";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("app/{id}/init")]
    public async Task<ApiResponse<string>> InitAppAsync([FromRoute] long id)
    {
        ApiResponse<string> response = new();
        try
        {
            var app = await _repository.GetAppAsync(id);
            if (app == null)
            {
                response.IsOk = false;
                response.Message = "Failed to find app.";
            }
            else
            {
                var execOption = new ExecOption
                {
                    Shell = app.ShellName,
                    ShellExtension = app.ShellExt,
                    ShellParameter = app.ShellOption,
                    Timeout = TimeSpan.FromHours(1)
                };

                var command = string.Empty;
                if (System.IO.File.Exists(app.InitScriptPath))
                {
                    command = await System.IO.File.ReadAllTextAsync(app.InitScriptPath);
                }

                if (string.IsNullOrEmpty(command))
                {
                    command = app.InitScript;
                }

                if (string.IsNullOrEmpty(command))
                {
                    response.IsOk = false;
                    response.Message = "No init script to execute.";
                }

                var output = await Exec.RunAsync(command);
                response.Content = output.Output;
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("app/{id}/start")]
    public async Task<ApiResponse<string>> StartAppAsync([FromRoute] long id)
    {
        ApiResponse<string> response = new();
        try
        {
            var app = await _repository.GetAppAsync(id);
            if (app == null)
            {
                response.IsOk = false;
                response.Message = "Failed to find app.";
            }
            else
            {
                var execOption = new ExecOption
                {
                    Shell = app.ShellName,
                    ShellExtension = app.ShellExt,
                    ShellParameter = app.ShellOption,
                    Timeout = TimeSpan.FromHours(1)
                };

                var command = string.Empty;
                if(System.IO.File.Exists(app.StartScriptPath))
                {
                    command = await System.IO.File.ReadAllTextAsync(app.StartScriptPath);
                }

                if(string.IsNullOrEmpty(command))
                {
                    command = app.StartScript;
                }

                if(string.IsNullOrEmpty(command))
                {
                    response.IsOk = false;
                    response.Message = "No start script to execute.";
                }

                var output = await Exec.RunAsync(command);
                response.Content = output.Output;
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("app/{id}/stop")]
    public async Task<ApiResponse<string>> StopAppAsync([FromRoute] long id)
    {
        ApiResponse<string> response = new();
        try
        {
            var app = await _repository.GetAppAsync(id);
            if (app == null)
            {
                response.IsOk = false;
                response.Message = "Failed to find app.";
            }
            else
            {
                var execOption = new ExecOption
                {
                    Shell = app.ShellName,
                    ShellExtension = app.ShellExt,
                    ShellParameter = app.ShellOption,
                    Timeout = TimeSpan.FromHours(1)
                };

                var command = string.Empty;
                if (System.IO.File.Exists(app.StopScriptPath))
                {
                    command = await System.IO.File.ReadAllTextAsync(app.StopScriptPath);
                }

                if (string.IsNullOrEmpty(command))
                {
                    command = app.StopScript;
                }

                if (string.IsNullOrEmpty(command))
                {
                    response.IsOk = false;
                    response.Message = "No stop script to execute.";
                }

                var output = await Exec.RunAsync(command);
                response.Content = output.Output;
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpDelete("task/{id}")]
    public async Task<ApiResponse<object>> DeleteTaskAsync([FromRoute] long id)
    {
        ApiResponse<object> response = new();
        try
        {
            await _repository.DeleteTaskAsync(id);
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("run/{id}/stop")]
    public ApiResponse<object> StopRun([FromRoute] long id)
    {
        ApiResponse<object> response = new();
        try
        {
            _commandManager.Stop(id);
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPut("run/{id}")]
    public async Task<ApiResponse<object>> RunTaskAsync([FromRoute] long id)
    {
        ApiResponse<object> response = new();
        try
        {
            var runId = await StartRunAsync(id);
            response.RedirectTo = $"/run/{runId}";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    private async Task<long> StartRunAsync(long taskId)
    {
        var task = await _repository.GetTaskAsync(taskId);
        PikaTaskRun run = new()
        {
            TaskId = taskId,
            Script = task.Script,
            ShellName = task.ShellName,
            ShellOption = task.ShellOption,
            ShellExt = task.ShellExt,
            Status = PikaTaskStatus.Pending,
            CreatedAt = DateTime.Now.Ticks
        };

        var runId = await _repository.AddTaskRunAsync(run);
        return runId;
    }

    [HttpPut("task/add")]
    public async Task<ApiResponse<object>> AddTaskAsync([FromForm] PikaTask task)
    {
        ApiResponse<object> response = new();
        try
        {
            task.CreatedAt = task.LastModifiedAt = DateTime.Now.Ticks;
            var isTemp = task.IsTemp || string.IsNullOrEmpty(task.Name);
            if (isTemp)
            {
                task.IsTemp = true;
                task.Name = $"temp_{Guid.NewGuid():N}";
                task.Description = "Temp one time task";
            }

            var id = await _repository.AddTaskAsync(task);
            if (isTemp)
            {
                var runId = await StartRunAsync(id);
                response.RedirectTo = $"/run/{runId}";
            }
            else
            {
                response.RedirectTo = $"/task/{id}";
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("task/update")]
    public async Task<ApiResponse<object>> UpdateTaskAsync([FromForm] PikaTask task)
    {
        ApiResponse<object> response = new();
        try
        {
            task.LastModifiedAt = DateTime.Now.Ticks;
            await _repository.UpdateTaskAsync(task);
            response.RedirectTo = $"/task/{task.Id}";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("shrinkDB")]
    public async Task<ApiResponse<object>> ShrinkDb()
    {
        ApiResponse<object> response = new();
        try
        {
            await _repository.VacuumDbAsync();
            response.RedirectTo = "/setting";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("run/{runId}/output")]
    public async Task<ApiResponse<TaskRunOutputViewModel>> GetRunOutputs([FromRoute] long runId,
        [FromQuery] long lastPoint)
    {
        ApiResponse<TaskRunOutputViewModel> response = new();
        try
        {
            var taskRun = await _repository.GetTaskRunAsync(runId);
            if (taskRun == null)
            {
                response.IsOk = false;
                response.Message = $"Run with id = {runId} not exist.";
            }
            else
            {
                var outputs = await _repository.GetTaskRunOutputs(runId, lastPoint, 100);
                outputs.Reverse();
                var maxTimestamp = default(DateTime).Ticks;
                var lastEl = outputs.LastOrDefault();
                if (lastEl != null)
                {
                    maxTimestamp = lastEl.CreatedAt;
                }

                maxTimestamp = Math.Max(lastPoint, maxTimestamp);
                response.Content = new TaskRunOutputViewModel
                {
                    LastPoint = maxTimestamp.ToString(),
                    Outputs = outputs,
                    CompletedAt = taskRun.GetCompletedAtHtml(),
                    StartedAt = taskRun.GetStartAtHtml(),
                    Status = taskRun.Status.ToString(),
                    Elapsed = taskRun.GetElapsedHtml()
                };
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPost("setting/update")]
    public async Task<ApiResponse<object>> UpdateSettingAsync([FromForm] PikaSetting setting)
    {
        ApiResponse<object> response = new();
        try
        {
            if (setting.ItemsPerPage is > 0 and <= 50)
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ItemsPerPage, setting.ItemsPerPage.ToString());
                _setting.ItemsPerPage = setting.ItemsPerPage;
            }
            else
            {
                response.IsOk = false;
                response.Message = "Invalid Items Per Page set: it must be in [1, 50].";
                return response;
            }

            if (setting.RetainSizeInMb > 0)
            {
                await _repository.InsertOrUpdateSetting(SettingKey.RetainSizeInMb, setting.RetainSizeInMb.ToString());
                _setting.RetainSizeInMb = setting.RetainSizeInMb;
            }
            else
            {
                response.IsOk = false;
                response.Message = "Invalid Retain Size In MB set: it must be greater than 0.";
                return response;
            }

            if (!string.IsNullOrEmpty(setting.DefaultShellName))
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ShellName, setting.DefaultShellName);
                _setting.DefaultShellName = setting.DefaultShellName;
            }
            else
            {
                response.IsOk = false;
                response.Message = "Invalid Default Shell Name set.";
                return response;
            }

            if (!string.IsNullOrEmpty(setting.DefaultShellExt))
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ShellExt, setting.DefaultShellExt);
                _setting.DefaultShellExt = setting.DefaultShellExt;
            }
            else
            {
                response.IsOk = false;
                response.Message = "Invalid Default Shell Extension set.";
                return response;
            }

            if (!string.IsNullOrEmpty(setting.DefaultShellOption))
            {
                await _repository.InsertOrUpdateSetting(SettingKey.ShellOptions, setting.DefaultShellOption);
                _setting.DefaultShellOption = setting.DefaultShellOption;
            }
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }
}