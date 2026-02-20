using CodifyProjectsBackend.Models;
using CodifyProjectsBackend.Models.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodifyProjectsBackend.Controllers;

public class AccountController : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Authors");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login([FromServices]AppDbContext db, [FromForm]LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var account = await db.Accounts.FirstOrDefaultAsync(a => a.Login == dto.Login);

        if (account == null)
        {
            return View("Index");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, account.PasswordHash))
        {
            return View("Index");
        }

        var claims = new List<Claim>()
        {
            new(ClaimTypes.Name, account.Login),
            new(ClaimTypes.NameIdentifier, account.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Cookies");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("Cookies", principal);

        return RedirectToAction("Index", "Authors");
    }

    public async Task<IActionResult> LogOut()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index");
    }
}
