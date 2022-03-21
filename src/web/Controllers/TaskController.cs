using Microsoft.AspNetCore.Mvc;

namespace Pika.Web.Controllers;

public class TaskController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}