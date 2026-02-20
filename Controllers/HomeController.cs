using CodifyProjectsBackend.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodifyProjectsBackend.Controllers;

public class HomeController : Controller
{
    [HttpGet]
    [Authorize]
    public IActionResult Index()
    {
        return RedirectToAction("Index", "Authors");
    }
}
