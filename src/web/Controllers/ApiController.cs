using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Command;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

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

    [HttpDelete("task/{id}")]
    public async Task<ApiResponse<object>> DeleteTaskAsync([FromRoute] long id)
    {
        var response = new ApiResponse<object>();
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
        var response = new ApiResponse<object>();
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
    public async Task<ApiResponse<object>> RunAsync([FromRoute] long id)
    {
        var response = new ApiResponse<object>();
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
        var run = new PikaTaskRun
        {
            TaskId = taskId,
            Script = task.Script,
            ShellName = task.ShellName,
            ShellOption = task.ShellOption,
            ShellExt = task.ShellExt,
            Status = PikaTaskStatus.Pending
        };

        var runId = await _repository.AddTaskRunAsync(run);
        return runId;
    }

    [HttpPut("task/add")]
    public async Task<ApiResponse<object>> AddTaskAsync([FromForm] PikaTask task)
    {
        var response = new ApiResponse<object>();
        try
        {
            var istemp = task.IsTemp || string.IsNullOrEmpty(task.Name);
            if (istemp)
            {
                task.IsTemp = true;
                task.Name = $"temp_{Guid.NewGuid().ToString("N")}";
                task.Description = "Temp one time task";
            }

            var id = await _repository.AddTaskAsync(task);
            if (istemp)
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
        var response = new ApiResponse<object>();
        try
        {
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
        var response = new ApiResponse<object>();
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
        var response = new ApiResponse<TaskRunOutputViewModel>();
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
                var outputs = await _repository.GetTaskRunOutputs(runId, new DateTime(lastPoint), 100);
                outputs.Reverse();
                var maxTimestamp = default(DateTime).Ticks;
                var lastEl = outputs.LastOrDefault();
                if (lastEl != null)
                {
                    maxTimestamp = lastEl.CreatedAt.Ticks;
                }

                maxTimestamp = Math.Max(lastPoint, maxTimestamp);
                response.Content = new TaskRunOutputViewModel
                {
                    LastPoint = maxTimestamp,
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
        var response = new ApiResponse<object>();
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