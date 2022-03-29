using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Lib.Util;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

[Route("api")]
[ApiController]
public class ApiController : ControllerBase
{
    private readonly IDbRepository _repository;
    private readonly ILogger<ApiController> _logger;

    public ApiController(IDbRepository repository, ILogger<ApiController> logger)
    {
        _logger = logger;
        _repository = repository;
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

    [HttpPut("run/{id}")]
    public async Task<ApiResponse<object>> RunAsync([FromRoute] long id)
    {
        var response = new ApiResponse<object>();
        try
        {
            var task = await _repository.GetTaskAsync(id);
            var run = new PikaTaskRun
            {
                TaskId = id,
                Script = task.Script,
                ShellName = task.ShellName,
                ShellOption = task.ShellOption,
                ShellExt = task.ShellExt,
                Status = PikaTaskStatus.Pending
            };
            var runId = await _repository.AddTaskRunAsync(run);
            response.RedirectTo = $"/run/{runId}";
        }
        catch (Exception ex)
        {
            response.IsOk = false;
            response.Message = ex.Message;
        }

        return response;
    }

    [HttpPut("task/add")]
    public async Task<ApiResponse<object>> AddTaskAsync([FromForm] PikaTask task)
    {
        var response = new ApiResponse<object>();
        try
        {
            var id = await _repository.AddTaskAsync(task);
            response.RedirectTo = $"/task/{id}";
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

    [HttpPost("export")]
    public async Task<IActionResult> ExportAsync()
    {
        var tasks = await _repository.GetTasksAsync(int.MaxValue, 0, orderByClause: "created_at ASC");
        var content =
            Encoding.UTF8.GetBytes(JsonUtil.Serialize(tasks, true));
        return File(content, "application/json", "pika-task.json");
    }

    [HttpPost("import")]
    public async Task<IActionResult> ImportAsync(IFormFile file)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            foreach (var pikaTask in await JsonUtil.DeserializeAsync<List<PikaTask>>(stream))
            {
                await _repository.AddTaskAsync(pikaTask);
            }

            return Redirect("~/task");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed.");
        }

        return BadRequest();
    }
}