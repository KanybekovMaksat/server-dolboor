using CodifyProjectsBackend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CodifyProjectsBackend.Controllers;

public class StatusController : Controller
{
    public IActionResult Index()
    {
        return Content("Server is working!");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
