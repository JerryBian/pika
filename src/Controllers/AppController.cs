using Microsoft.AspNetCore.Mvc;
using Pika.Common.Model;
using Pika.Common.Store;

namespace Pika.Controllers;

public class AppController : Controller
{
    private readonly IDbRepository _repository;
    private readonly PikaSetting _setting;

    public AppController(PikaSetting setting, IDbRepository repository)
    {
        _setting = setting;
        _repository = repository;
    }

    [HttpGet("/app")]
    public async Task<IActionResult> Index([FromQuery] int page = 1)
    {
        var apps = await _repository.GetAppsAsync(orderByClause: "created_at DESC");
        return View(apps);
    }

    [HttpGet("/app/{id}")]
    public async Task<IActionResult> Detail([FromRoute] long id)
    {
        var app = await _repository.GetAppAsync(id);
        if (app == null)
        {
            return NotFound();
        }

        return View(app);
    }

    [HttpGet("/app/add")]
    public async Task<IActionResult> Add()
    {
        return View();
    }

    [HttpGet("/app/{id}/update")]
    public async Task<IActionResult> Update([FromRoute] long id)
    {
        var task = await _repository.GetAppAsync(id);
        return task == null ? NotFound() : View(task);
    }
}