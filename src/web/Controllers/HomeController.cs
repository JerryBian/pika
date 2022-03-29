using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pika.Lib.Store;
using Pika.Web.Models;

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
        var status = await _repository.GetSystemStatusAsync();
        return View(status);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}