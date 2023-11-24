using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pika.Lib.Extension;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Lib.Util;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Pika.Web.Controllers;

public class SettingController : Controller
{
    private readonly ILogger<SettingController> _logger;
    private readonly IDbRepository _repository;
    private PikaSetting _setting;

    public SettingController(PikaSetting setting, IDbRepository repository, ILogger<SettingController> logger)
    {
        _setting = setting;
        _logger = logger;
        _repository = repository;
    }

    public IActionResult Index()
    {
        ViewData["DbSize"] = _repository.GetDbSize().ToByteSizeHuman();
        return View(_setting);
    }

    [HttpPost("/setting/export")]
    public async Task<IActionResult> ExportAsync()
    {
        var tasks = await _repository.GetTasksAsync(orderByClause: "created_at ASC", whereClause: "is_temp = 0");
        PikaExport export = new()
        {
            Setting = _setting,
            Tasks = tasks
        };
        var content =
            Encoding.UTF8.GetBytes(JsonUtil.Serialize(export, true));
        return File(content, "application/json", "pika-export.json");
    }

    [HttpPost("/setting/import")]
    public async Task<IActionResult> ImportAsync(IFormFile file)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var export = await JsonUtil.DeserializeAsync<PikaExport>(stream);
            if (export.Setting != null)
            {
                if (export.Setting.RetainSizeInMb < 1)
                {
                    export.Setting.RetainSizeInMb = _setting.RetainSizeInMb;
                }

                if (export.Setting.ItemsPerPage is < 1 or > 50)
                {
                    export.Setting.ItemsPerPage = _setting.ItemsPerPage;
                }

                _setting = export.Setting;
            }

            if (export.Tasks != null)
            {
                foreach (var pikaTask in export.Tasks)
                {
                    pikaTask.CreatedAt = DateTime.Now.Ticks;
                    pikaTask.LastModifiedAt = DateTime.Now.Ticks;
                    _ = await _repository.AddTaskAsync(pikaTask);
                }
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