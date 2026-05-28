using System.Diagnostics;
using ASP_MVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace ASP_MVC.Controllers;

/// <summary>
/// PoCトップ画面と共通ページを返すController。
/// </summary>
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

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
