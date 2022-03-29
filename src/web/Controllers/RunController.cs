using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;

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
        var whereClause = "";
        if (Enum.TryParse<PikaTaskStatus>(status, out var s))
        {
            whereClause = $"status={(int) s}";
        }

        var runsCount = await _repository.GetRunsCountAsync();
        var pagedViewModel = new PagedViewModel<RunDetailViewModel>(page, runsCount, _setting.ItemsPerPage);
        var runs = await _repository.GetTaskRunsAsync(_setting.ItemsPerPage,
            (pagedViewModel.CurrentPage - 1) * _setting.ItemsPerPage, whereClause, "created_at DESC");
        foreach (var run in runs)
        {
            var runDetailViewModel = new RunDetailViewModel {Run = run};
            var vm = pagedViewModel.Items.FirstOrDefault(x => x.Run.TaskId == run.TaskId);
            if (vm == null)
            {
                var task = await _repository.GetTaskAsync(run.TaskId);
                runDetailViewModel.TaskName = task.Name;
            }
            else
            {
                runDetailViewModel.TaskName = vm.TaskName;
            }

            pagedViewModel.Items.Add(runDetailViewModel);
        }

        pagedViewModel.Url = Request.GetDisplayUrl();
        return View(pagedViewModel);
    }

    [HttpGet("/run/{id}")]
    public async Task<IActionResult> Detail([FromRoute] long id)
    {
        var taskRun = await _repository.GetTaskRunAsync(id);
        if (taskRun == null)
        {
            return NotFound();
        }

        var task = await _repository.GetTaskAsync(taskRun.TaskId);
        if (task == null)
        {
            return NotFound();
        }

        var viewModel = new TaskRunDetailViewModel
        {
            Task = task,
            Run = taskRun
        };
        return View(viewModel);
    }

    [HttpGet("/run/{id}/output/raw")]
    public async Task<IActionResult> RawOutput([FromRoute] long id)
    {
        var outputs = await _repository.GetTaskRunOutputs(id);
        outputs.Reverse();

        return Content(string.Join(Environment.NewLine, outputs.Select(x => x.Message)), "text/plain", Encoding.UTF8);
    }
}