using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

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
        var pagedViewModel = new PagedViewModel<TaskDetailViewModel>(page, tasksCount, _setting.ItemsPerPage);
        var tasks = await _repository.GetTasksAsync(_setting.ItemsPerPage,
            (pagedViewModel.CurrentPage - 1) * _setting.ItemsPerPage, orderByClause: "created_at DESC");
        foreach (var pikaTask in tasks)
        {
            var taskDetailViewModel = new TaskDetailViewModel {Task = pikaTask};
            taskDetailViewModel.RunCount = await _repository.GetRunsCountAsync(pikaTask.Id);
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

        var taskDetailViewModel = new TaskDetailViewModel {Task = task};
        var runs = await _repository.GetTaskRunsAsync(100, 0, $"task_id={id}", "created_at DESC");
        taskDetailViewModel.Runs = runs;
        taskDetailViewModel.RunCount = await _repository.GetRunsCountAsync(task.Id);
        return View(taskDetailViewModel);
    }

    [HttpGet("/task/add")]
    public async Task<IActionResult> Add([FromQuery] bool isTemp, [FromQuery] long sourceTaskId)
    {
        var model = new TaskAddViewModel
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
        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }
}