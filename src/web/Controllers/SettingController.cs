using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Lib.Util;

namespace Pika.Web.Controllers;

public class SettingController : Controller
{
    private readonly PikaSetting _setting;
    private readonly IDbRepository _repository;
    private readonly ILogger<SettingController> _logger;

    public SettingController(PikaSetting setting, IDbRepository repository, ILogger<SettingController> logger)
    {
        _setting = setting;
        _logger = logger;
        _repository = repository;
    }

    public IActionResult Index()
    {
        return View(_setting);
    }

    [HttpPost("/setting/export")]
    public async Task<IActionResult> ExportAsync()
    {
        var tasks = await _repository.GetTasksAsync(int.MaxValue, 0, orderByClause: "created_at ASC");
        var content =
            Encoding.UTF8.GetBytes(JsonUtil.Serialize(tasks, true));
        return File(content, "application/json", "pika-task.json");
    }

    [HttpPost("/setting/import")]
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