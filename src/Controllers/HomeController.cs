using Microsoft.AspNetCore.Mvc;
using Pika.Common.Command;
using Pika.Common.Model;
using Pika.Common.Store;
using Pika.Common.Util;
using Pika.Models;
using System.Diagnostics;

namespace Pika.Controllers;

public class HomeController : Controller
{
    private readonly IDbRepository _repository;

    public HomeController(IDbRepository repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel();
        model.Apps.AddRange(await _repository.GetAppsAsync());
        model.Tasks.AddRange(await _repository.GetTasksAsync());
        model.SystemStatus = await _repository.GetSystemStatusAsync();

        //var foo = await RemoteCommandExecutor.RunAsync("lsblk -O -J",CancellationToken.None);
        //var j = JsonUtil.Deserialize<PikaDriveLsblk>(foo.Output);
        //var x = j.BlockDevices.Where(d => d.Type == "disk").ToList();
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}