using Microsoft.AspNetCore.Mvc;

namespace Pika.Web.Controllers;

public class RunController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}