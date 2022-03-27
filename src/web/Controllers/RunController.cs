using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Store;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

public class RunController : Controller
{
    private readonly IDbRepository _repository;

    public RunController(IDbRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Index()
    {
        return View();
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
            CreatedAt = taskRun.CreatedAt,
            CreatedAtDisplay = taskRun.CreatedAt.Humanize(),
            RunId = id,
            Script = taskRun.Script,
            ShellExt = taskRun.ShellExt,
            ShellName = taskRun.ShellName,
            ShellOption = taskRun.ShellOption,
            TaskId = task.Id,
            TaskName = task.Name
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