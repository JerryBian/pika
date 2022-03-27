using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly PikaSetting _setting;
    private readonly IDbRepository _repository;

    public ApiController(IDbRepository repository, PikaSetting setting)
    {
        _setting = setting;
        _repository = repository;
    }

    [HttpGet("task/{taskId}")]
    public async Task<PikaTask> GetTaskAsync([FromRoute] long taskId)
    {
        var task = await _repository.GetTaskAsync(taskId);
        return task;
    }

    [HttpPut("task")]
    public async Task AddTaskAsync(PikaTask task)
    {
        await _repository.AddTaskAsync(task);
    }

    [HttpPost("task")]
    public async Task UpdateTaskAsync(PikaTask task)
    {
        await _repository.UpdateTaskAsync(task);
    }

    [HttpPut("run")]
    public async Task<long> RunAsync(PikaTaskRun run)
    {
        var task = await _repository.GetTaskAsync(run.TaskId);
        run.Script = task.Script;
        run.ShellName = _setting.DefaultShellName;
        run.ShellOption = _setting.DefaultShellOptions;
        run.ShellExt = _setting.DefaultShellExt;
        run.Status = PikaTaskStatus.Pending;
        var runId = await _repository.AddTaskRunAsync(run);
        return runId;
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
                    CompletedAt = taskRun.CompletedAt == default ? "-" : taskRun.CompletedAt.Humanize(false),
                    CompletedAtTooltip = taskRun.CompletedAt == default ? "Not completed yet" : taskRun.CompletedAt.ToString(CultureInfo.InvariantCulture),
                    StartedAt = taskRun.StartedAt == default ? "-" : taskRun.StartedAt.Humanize(false),
                    StartedAtTooltip = taskRun.StartedAt == default ? "Not started yet" : taskRun.StartedAt.ToString(CultureInfo.InvariantCulture),
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
}