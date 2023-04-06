using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ASP_NET.Models;

namespace ASP_NET.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }
    
    public IActionResult Intro()
    {
        return View();
    }
    public IActionResult Scheme()
    {
        ViewBag.bagdata = "Data from Bag";
        ViewData["viewdata"] = "Data from Dara";
        return View();
    }

    public IActionResult Razor()
    {
        return View();
    }
    public IActionResult Model()
    {
        Models.Home.Model model = new()
        {
            Header = "Модели",
            Title = "Передача модели представлению",
            Departments = new()
            {
                "Department 1",
                "Department 2",
                "Department 3",
                "Department 4",
                "Department 5"
            },
            Products = new()
            {
                new() {Name = "Отвертка", Price = 30.50},
                new() {Name = "Дрель", Price = 3000.99},
                new() {Name = "TV", Price = 30000.99},
                new() {Name = "Galaxy S20", Price = 25000.99},
                new() {Name = "Гвоздь", Price = 3.99},
                new() {Name = "Galaxy Buds Pro", Price = 5500.99}
            }
        };
        return View(model);
    }
    public IActionResult URL()
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