using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Store;
using Pika.Web.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pika.Web.Controllers;

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

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}