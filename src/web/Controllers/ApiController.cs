using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly IDbRepository _repository;

    public ApiController(IDbRepository repository)
    {
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

    [HttpPut("run/{taskId}")]
    public async Task<long> RunAsync([FromRoute] long taskId)
    {
        var runId = await _repository.AddTaskRunAsync(taskId);
        return runId;
    }

    [HttpGet("run/{runId}/output")]
    public async Task<TaskRunOutputViewModel> GetRunOutputs([FromRoute] long runId, [FromQuery] long laterThan)
    {
        var taskRun = await _repository.GetTaskRunAsync(runId);
        var task = await _repository.GetTaskAsync(taskRun.TaskId);
        var outputs = await _repository.GetTaskRunOutputs(runId, new DateTime(laterThan));
        var maxTimestamp = default(DateTime).Ticks;
        var lastEl = outputs.LastOrDefault();
        if (lastEl != null)
        {
            maxTimestamp = lastEl.CreatedAt.Ticks;
        }

        maxTimestamp = Math.Max(laterThan, maxTimestamp);
        outputs.Reverse();
        return new TaskRunOutputViewModel
        {
            MaxTimestamp = maxTimestamp,
            Outputs = outputs,
            RunId = runId,
            TaskId = task.Id,
            RunEndAt = taskRun.CompletedAt == default ? string.Empty : taskRun.CompletedAt.ToString(),
            RunStartAt = taskRun.CreatedAt.ToString(),
            Status = taskRun.Status.ToString(),
            TaskName = task.Name,
            Foo = string.Join(Environment.NewLine, outputs.Select(x => x.Message))
        };
    }

}