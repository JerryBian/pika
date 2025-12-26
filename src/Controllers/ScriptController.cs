using Microsoft.AspNetCore.Mvc;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Models;
using System.Text;

namespace Pika.Controllers
{
    [Route("script")]
    public class ScriptController : Controller
    {
        private readonly IPikaStore _repository;
        private readonly PikaSetting _setting;

        public ScriptController(IPikaStore repository, PikaSetting setting)
        {
            _repository = repository;
            _setting = setting;
        }

        public async Task<IActionResult> Index()
        {
            var model = new PikaScriptIndexViewModel();
            var scripts = await _repository.GetScriptsAsync();
            var runs = await _repository.GetScriptRunsByStatusAsync(PikaScriptStatus.Running);
            foreach (var item in scripts)
            {
                item.RunningCount = runs.Count(x => x.ScriptId == item.Id);
            }

            model.SavedScripts = [.. scripts.OrderBy(x => x.Name)];
            return View(model);
        }

        [HttpGet("add")]
        public async Task<IActionResult> AddScript([FromQuery] bool isTemp, [FromQuery] long sourceTaskId)
        {
            PikaScriptNewScriptViewModel model = new()
            {
                IsTemp = isTemp
            };

            PikaScript sourceTask = null;
            if (sourceTaskId > 0)
            {
                sourceTask = await _repository.GetScriptAsync(sourceTaskId);
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

            return View("AddScript", model);
        }

        [HttpGet("{id}/update")]
        public async Task<IActionResult> UpdateScript([FromRoute] long id)
        {
            var task = await _repository.GetScriptAsync(id);
            return task == null ? NotFound() : View("UpdateScript", task);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ScriptDetail([FromRoute] long id)
        {
            var script = await _repository.GetScriptAsync(id);
            if (script == null)
            {
                return NotFound();
            }

            PikaScriptDetailViewModel model = new() { Script = script };
            var runs = await _repository.GetScriptRunsByScriptIdAsync(id, 30);
            model.Runs = runs;
            return View("ScriptDetail", model);
        }

        [HttpGet("run/{id}")]
        public async Task<IActionResult> ScriptRun([FromRoute] long id, [FromQuery] string format)
        {
            if(string.Equals(format, "raw", StringComparison.OrdinalIgnoreCase))
            {
                var outputs = await _repository.GetScriptRunOutputs(id, desc: false);
                return Content(string.Join(Environment.NewLine, outputs.Select(x => x.Message)), "text/plain", Encoding.UTF8);
            }

            var taskRun = await _repository.GetScriptRunAsync(id);
            if (taskRun == null)
            {
                return NotFound();
            }

            var task = await _repository.GetScriptAsync(taskRun.ScriptId);
            if (task == null)
            {
                return NotFound();
            }

            PikaScriptRunViewModel viewModel = new()
            {
                Script = task,
                Run = taskRun
            };
            return View(viewModel);
        }
    }
}
