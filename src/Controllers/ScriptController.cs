using Microsoft.AspNetCore.Mvc;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Models;

namespace Pika.Controllers
{
    [Route("script")]
    public class ScriptController : Controller
    {
        private readonly IDbRepository _repository;
        private readonly PikaSetting _setting;

        public ScriptController(IDbRepository repository, PikaSetting setting)
        {
            _repository = repository;
            _setting = setting;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("add")]
        public async Task<IActionResult> AddScript([FromQuery] bool isTemp, [FromQuery] long sourceTaskId)
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

            return View("AddScript", model);
        }
    }
}
