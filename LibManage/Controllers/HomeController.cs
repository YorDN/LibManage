using LibManage.Web.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Diagnostics;

namespace LibManage.Web.Controllers;

[AllowAnonymous]
public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
    [HttpGet]
    public IActionResult Privacy()
    {
        return View();
    }
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
