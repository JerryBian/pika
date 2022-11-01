using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pika.Web.Controllers;

public class RunController : Controller
{
    private readonly IDbRepository _repository;
    private readonly PikaSetting _setting;

    public RunController(PikaSetting setting, IDbRepository repository)
    {
        _setting = setting;
        _repository = repository;
    }

    [HttpGet("/run")]
    public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] string status = "")
    {
        string whereClause = "";
        if (Enum.TryParse<PikaTaskStatus>(status, out PikaTaskStatus s))
        {
            whereClause = $"status={(int)s}";
        }

        int runsCount = await _repository.GetRunsCountAsync(whereClause);
        PagedViewModel<RunDetailViewModel> pagedViewModel = new(page, runsCount, _setting.ItemsPerPage);
        System.Collections.Generic.List<PikaTaskRun> runs = await _repository.GetTaskRunsAsync(_setting.ItemsPerPage,
            (pagedViewModel.CurrentPage - 1) * _setting.ItemsPerPage, whereClause, "created_at DESC");
        foreach (PikaTaskRun run in runs)
        {
            RunDetailViewModel runDetailViewModel = new() { Run = run };
            RunDetailViewModel vm = pagedViewModel.Items.FirstOrDefault(x => x.Run.TaskId == run.TaskId);
            if (vm == null)
            {
                PikaTask task = await _repository.GetTaskAsync(run.TaskId);
                runDetailViewModel.TaskName = task.Name;
            }
            else
            {
                runDetailViewModel.TaskName = vm.TaskName;
            }

            pagedViewModel.Items.Add(runDetailViewModel);
        }

        pagedViewModel.Url = Request.Path;
        return View(pagedViewModel);
    }

    [HttpGet("/run/{id}")]
    public async Task<IActionResult> Detail([FromRoute] long id)
    {
        PikaTaskRun taskRun = await _repository.GetTaskRunAsync(id);
        if (taskRun == null)
        {
            return NotFound();
        }

        PikaTask task = await _repository.GetTaskAsync(taskRun.TaskId);
        if (task == null)
        {
            return NotFound();
        }

        TaskRunDetailViewModel viewModel = new()
        {
            Task = task,
            Run = taskRun
        };
        return View(viewModel);
    }

    [HttpGet("/run/{id}/output/raw")]
    public async Task<IActionResult> RawOutput([FromRoute] long id)
    {
        System.Collections.Generic.List<PikaTaskRunOutput> outputs = await _repository.GetTaskRunOutputs(id, desc: false);
        return Content(string.Join(Environment.NewLine, outputs.Select(x => x.Message)), "text/plain", Encoding.UTF8);
    }
}