using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pika.Common.Extension;
using Pika.Common.Store;
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

    public IActionResult Index()
    {
        var dbSize = _repository.GetDbSize();
        ViewData["DbSize"] = dbSize.ToByteSizeHuman();
        return View();
    }

    [Route("/error")]
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}