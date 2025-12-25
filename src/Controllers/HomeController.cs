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
    private readonly IPikaStore _repository;

    public HomeController(IPikaStore repository)
    {
        _repository = repository;
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }

    [Route("/error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}