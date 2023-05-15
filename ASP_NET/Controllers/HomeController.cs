using System.Diagnostics;
using ASP_NET.Data;
using Microsoft.AspNetCore.Mvc;
using ASP_NET.Models;
using ASP_NET.Services;
using ASP_NET.Services.Hash;

namespace ASP_NET.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TimeService _timeService;
    private readonly DateService _dateService;
    private readonly DtService _dtService;
    private readonly IHashService _hashService;
    private readonly DataContext _dataContext;
    private readonly IConfiguration _configuration;


    public HomeController(ILogger<HomeController> logger, TimeService timeService, DateService dateService, DtService dtService, IHashService hashService, DataContext dataContext, IConfiguration configuration)
    {
        _logger = logger;
        _timeService = timeService;
        _dateService = dateService;
        _dtService = dtService;
        _hashService = hashService;
        _dataContext = dataContext;
        _configuration = configuration;
    }
    
    public IActionResult EmailConfirmation()
    {
        ViewData["config_host"] = _configuration["Smtp:Gmail:Host"];
        ViewData["config_port"] = _configuration["Smtp:Gmail:Port"];
        ViewData["config_email"] = _configuration["Smtp:Gmail:Email"];
        ViewData["config_ssl"] = _configuration["Smtp:Gmail:Ssl"];
        return View();
    }
    
    public IActionResult Middleware()
    {
        return View();
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
    public ViewResult Services()
    {
        ViewData["now"] = _timeService.GetTime();
        ViewData["hashCode"] = _timeService.GetHashCode();

        ViewData["date_now"] = _dateService.GetDate();
        ViewData["date_hashCode"] = _dateService.GetHashCode();

        ViewData["dt_now"] = _dtService.GetNow();
        ViewData["dt_hashCode"] = _dtService.GetHashCode();

        ViewData["hash"] = _hashService.Hash("123");

        return View();
    }

    public ViewResult Context()
    {
        ViewData["UsersCount"] = _dataContext.Users.Count();
        return View();
    }

    public IActionResult Sessions(String? number)
    {
        _logger.LogInformation("number: " + number);
        if (number is not null)
        {
            HttpContext.Session.SetString("number", number);
        }

        ViewData["number"] = HttpContext.Session.GetString("number");
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

    public ViewResult WebApi()
    {
        return View();
    }
}