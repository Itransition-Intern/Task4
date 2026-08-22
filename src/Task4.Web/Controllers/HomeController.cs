using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task4.Web.Models;

namespace Task4.Web.Controllers;

public class HomeController : Controller
{
    // Anonim foydalanuvchi faqat login/register formasini ko'rishi kerak,
    // shuning uchun bosh sahifa to'g'ridan-to'g'ri login'ga yo'naltiradi.
    // Autentifikatsiyadan o'tgan foydalanuvchi esa Users jadvaliga boradi.
    public IActionResult Index()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToAction("Index", "Users")
            : RedirectToAction("Login", "Account");
    }

    [Authorize]
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}