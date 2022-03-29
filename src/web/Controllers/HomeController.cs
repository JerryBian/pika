using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pika.Lib;
using Pika.Lib.Model;
using Pika.Lib.Store;
using Pika.Lib.Util;
using Pika.Web.Models;

namespace Pika.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDbRepository _repository;

    public HomeController(ILogger<HomeController> logger, IDbRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var status = await _repository.GetSystemStatusAsync();
        return View(status);
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }

    [HttpPost("/export")]
    public async Task<IActionResult> ExportAsync()
    {
        var tasks = await _repository.GetTasksAsync(int.MaxValue, 0, orderByClause: "created_at ASC");
        var content =
            Encoding.UTF8.GetBytes(JsonSerializer.Serialize(tasks, new JsonSerializerOptions {WriteIndented = true}));
        return File(content, "application/json", "pika_export.json");
    }

    [HttpPost("/import")]
    public async Task<IActionResult> ImportAsync(IFormFile file)
    {
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            var content = await reader.ReadToEndAsync();
            foreach (var pikaTask in JsonSerializer.Deserialize<List<PikaTask>>(content))
            {
                await _repository.AddTaskAsync(pikaTask);
            }
        }

        return Redirect("~/task");
    }
}