using Microsoft.AspNetCore.Mvc;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Models;
using System.Threading.Tasks;

namespace Pika.Controllers;

public class TaskController : Controller
{
    private readonly IDbRepository _repository;
    private readonly PikaSetting _setting;

    public TaskController(PikaSetting setting, IDbRepository repository)
    {
        _setting = setting;
        _repository = repository;
    }

    [HttpGet("/task")]
    public async Task<IActionResult> Index([FromQuery] int page = 1)
    {
        var tasksCount = await _repository.GetTasksCountAsync();
        PagedViewModel<TaskDetailViewModel> pagedViewModel = new(page, tasksCount, _setting.ItemsPerPage);
        var tasks = await _repository.GetTasksAsync(_setting.ItemsPerPage,
            (pagedViewModel.CurrentPage - 1) * _setting.ItemsPerPage, orderByClause: "created_at DESC");
        foreach (var pikaTask in tasks)
        {
            TaskDetailViewModel taskDetailViewModel = new()
            {
                Task = pikaTask,
                RunCount = await _repository.GetRunsCountAsync(pikaTask.Id)
            };
            pagedViewModel.Items.Add(taskDetailViewModel);
        }

        pagedViewModel.Url = Request.Path;
        return View(pagedViewModel);
    }

    [HttpGet("/task/{id}")]
    public async Task<IActionResult> Detail([FromRoute] long id)
    {
        var task = await _repository.GetTaskAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        TaskDetailViewModel taskDetailViewModel = new() { Task = task };
        var runs = await _repository.GetTaskRunsAsync(100, 0, $"task_id={id}", "created_at DESC");
        taskDetailViewModel.Runs = runs;
        taskDetailViewModel.RunCount = await _repository.GetRunsCountAsync(task.Id);
        return View(taskDetailViewModel);
    }

    [HttpGet("/task/add")]
    public async Task<IActionResult> Add([FromQuery] bool isTemp, [FromQuery] long sourceTaskId)
    {
        PikaScriptNewScriptViewModel model = new()
        {
            IsTemp = isTemp
        };

        PikaTask sourceTask = null;
        if (sourceTaskId > 0)
        {
            sourceTask = await _repository.GetTaskAsync(sourceTaskId);
        }

        if (sourceTask != null)
        {
            model.ShellExt = sourceTask.ShellExt;
            model.ShellName = sourceTask.ShellName;
            model.ShellOption = sourceTask.ShellOption;
            model.Script = sourceTask.Script;
            model.TaskName = $"(Clone) {sourceTask.Name}";
            model.TaskDescription = $"(Clone) {sourceTask.Description}";
        }
        else
        {
            model.ShellExt = _setting.DefaultShellExt;
            model.ShellName = _setting.DefaultShellName;
            model.ShellOption = _setting.DefaultShellOption;
        }

        return View(model);
    }

    [HttpGet("/task/{id}/update")]
    public async Task<IActionResult> Update([FromRoute] long id)
    {
        var task = await _repository.GetTaskAsync(id);
        return task == null ? NotFound() : View(task);
    }
}