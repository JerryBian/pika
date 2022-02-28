using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pika.Lib;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Lib.Util;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDbRepository _repository;

    public HomeController(ILogger<HomeController> logger, IDbRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var status = await _repository.GetSystemStatusAsync();
        return View(status);
    }

    [HttpGet("/task")]
    public async Task<IActionResult> Task([FromQuery] int p = 1)
    {
        var c = 10;
        if (Request.Cookies.ContainsKey(Constants.ItemsPerPage) &&
            int.TryParse(Request.Cookies[Constants.ItemsPerPage], out var r) && r > 0)
        {
            c = r;
        }

        var whereClause = string.Empty;
        if (Request.Cookies.ContainsKey(Constants.FilterBy) &&
            StringUtil.EqualsIgnoreCase(Request.Cookies[Constants.FilterBy], Constants.FilterByFavorite))
        {
            whereClause = "is_favorite=1";
        }

        var tasks = await _repository.GetTasksAsync(c, (p - 1) * c, whereClause, "created_at DESC");
        return View(tasks);
    }

    [HttpGet("/run")]
    public async Task<IActionResult> Run([FromQuery] int p = 1)
    {
        var c = 10;
        if (Request.Cookies.ContainsKey(Constants.ItemsPerPage) &&
            int.TryParse(Request.Cookies[Constants.ItemsPerPage], out var r) && r > 0)
        {
            c = r;
        }

        var whereClause = string.Empty;
        if (Request.Cookies.ContainsKey(Constants.FilterBy))
        {
            var filterBy = Request.Cookies[Constants.FilterBy];
            if (StringUtil.EqualsIgnoreCase(filterBy, Constants.FilterByRunning))
            {
                whereClause = $"status={(int) PikaTaskStatus.Running}";
            }
            else if(StringUtil.EqualsIgnoreCase(filterBy, Constants.FilterByPending))
            {
                whereClause = $"status={(int)PikaTaskStatus.Pending}";
            }
            else if (StringUtil.EqualsIgnoreCase(filterBy, Constants.FilterByCompleted))
            {
                whereClause = $"status={(int)PikaTaskStatus.Completed}";
            }
            else if (StringUtil.EqualsIgnoreCase(filterBy, Constants.FilterByDead))
            {
                whereClause = $"status={(int)PikaTaskStatus.Dead}";
            }
        }

        var orderByClause = "created_at DESC";
        if (Request.Cookies.ContainsKey(Constants.SortBy))
        {
            var orderBy = Request.Cookies[Constants.SortBy];
            if (StringUtil.EqualsIgnoreCase(orderBy, Constants.SortByCompletedAtDesc))
            {
                orderByClause = "completed_at DESC";
            }
            else if (StringUtil.EqualsIgnoreCase(orderBy, Constants.SortByRunElapsedDesc))
            {
                orderByClause = "(julianday(IFNULL(completed_at, DATETIME('now', 'localtime'))) - julianday(created_at)) DESC";
            }
        }

        var runs = await _repository.GetTaskRunsAsync(c, (p - 1) * c, whereClause, orderByClause);
        return View(runs);
    }

    [HttpGet("/run/{runId}")]
    public async Task<IActionResult> PeekRunOutput([FromRoute] long runId)
    {
        var taskRun = await _repository.GetTaskRunAsync(runId);
        var task = await _repository.GetTaskAsync(taskRun.TaskId);
        var outputs = await _repository.GetTaskRunOutputs(runId, default);
        var maxTimestamp = default(DateTime).Ticks;
        var lastEl = outputs.LastOrDefault();
        if (lastEl != null)
        {
            maxTimestamp = lastEl.CreatedAt.Ticks;
        }

        var viewModel = new TaskRunOutputViewModel
        {
            MaxTimestamp = maxTimestamp,
            Outputs = outputs,
            RunId = runId,
            TaskId = task.Id,
            RunEndAt = taskRun.CompletedAt == default ? string.Empty : taskRun.CompletedAt.ToString(),
            RunStartAt = taskRun.CreatedAt.ToString(),
            Status = taskRun.Status.ToString(),
            TaskName = task.Name,
            Script = taskRun.Script
        };

        return View("RunDetail", viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}