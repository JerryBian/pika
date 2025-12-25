using Microsoft.AspNetCore.Mvc;
using Pika.Common.Extension;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Common.Util;
using System.Text;

namespace Pika.Controllers;

[Route("setting")]
public class SettingController : Controller
{
    private readonly ILogger<SettingController> _logger;
    private readonly IPikaStore _repository;
    private PikaSetting _setting;

    public SettingController(
        PikaSetting setting, 
        IPikaStore repository, 
        ILogger<SettingController> logger)
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

    [HttpPost("export")]
    public async Task<IActionResult> ExportAsync()
    {
        var tasks = await _repository.GetScriptsAsync();
        PikaExport export = new()
        {
            Setting = _setting,
            Scripts = tasks,
            Apps = await _repository.GetAppsAsync()
        };
        var content =
            Encoding.UTF8.GetBytes(JsonUtil.Serialize(export, true));
        return File(content, "application/json", "pika-export.json");
    }

    [HttpPost("import")]
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

                _setting = export.Setting;
            }

            if (export.Scripts != null)
            {
                foreach (var PikaScript in export.Scripts)
                {
                    PikaScript.CreatedAt = DateTime.Now.Ticks;
                    PikaScript.LastModifiedAt = DateTime.Now.Ticks;
                    await _repository.AddScriptAsync(PikaScript);
                }
            }

            if(export.Apps != null)
            {
                foreach(var app in export.Apps)
                {
                    app.CreatedAt = app.LastModifiedAt = DateTime.Now.Ticks;
                    await _repository.AddAppAsync(app);
                }
            }

            return Redirect("~/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import failed.");
        }

        return BadRequest();
    }
}