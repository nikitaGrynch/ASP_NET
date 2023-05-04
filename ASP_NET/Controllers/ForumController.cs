using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ASP_NET.Data;
using ASP_NET.Data.Entity;
using ASP_NET.Models.Forum;
using ASP_NET.Services.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace ASP_NET.Controllers;

public class ForumController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<ForumController> _logger;
    private readonly IValidationService _validationService;
    private int Counter
    {
        get => _counter++;
        set { _counter = value; }
    }

    private int _counter;

    public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
    {
        _dataContext = dataContext;
        _logger = logger;
        _validationService = validationService;
        _counter = 0;
    }

    // GET
    public IActionResult Index()
    {
        ForumIndexModel model = new()
        {
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            Sections = _dataContext.Sections
                .Include(s => s.Author)
                .OrderBy(s => s.CreatedDt)
                .AsEnumerable()
                .Select(s => new ForumSectionModel()
            {
                Id = s.Id,
                Title = s.Title,
                Description = s.Description,
                Logo = $"/imgs/logos/section{Counter}.png",
                CreatedDtString = DateTime.Today == s.CreatedDt.Date ? 
                    "Today " + s.CreatedDt.ToString("HH:mm") 
                    : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                AuthorName = s.Author.RealName,
                AuthorAvatar = s.Author.Avatar is null ? null : $"/avatars/{s.Author.Avatar}"
            }).ToList(),
        };
        if (HttpContext.Session.GetString("CreateMessage") is String message)
        {
            model.CreateMessage = message;
            model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") != 0;

            if (model.IsMessagePositive == false)
            {
                model.FormModel = new()
                {
                    Title = HttpContext.Session.GetString("SectionTitle")!,
                    Description = HttpContext.Session.GetString("SectionDescription")!
                };
                HttpContext.Session.Remove("SectionTitle");
                HttpContext.Session.Remove("SectionDescription");
            }

            HttpContext.Session.Remove("CreateMessage");
            HttpContext.Session.Remove("IsMessagePositive");
        }
        return View(model);
    }
    
    
    
    [HttpPost]
    public RedirectToActionResult CreateSection([FromForm] ForumSectionFormModel model)
    {
        _logger.LogInformation("Title: {t}, Description: {d}", model.Title, model.Description);
        if (!_validationService.Validate(model.Title, ValidationTerms.NotEmpty)
            || !_validationService.Validate(model.Description, ValidationTerms.NotEmpty))
        {
            HttpContext.Session.SetString("CreateMessage" ,"Title cannot be empty");
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("SectionTitle", model.Title ?? String.Empty);
            HttpContext.Session.SetString("SectionDescription", model.Description ?? String.Empty);

        }
        else
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value!);
                _dataContext.Sections.Add(new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Title = model.Title,
                    Description = model.Description,
                    CreatedDt = DateTime.Now
                });
                
                _dataContext.SaveChanges();
                HttpContext.Session.SetString("CreateMessage" ,"Section successfully created");
                HttpContext.Session.SetInt32("IsMessagePositive", 1);
            }
            catch
            {
                HttpContext.Session.SetString("CreateMessage" ,"Auth error");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("SectionTitle", model.Title ?? String.Empty);
                HttpContext.Session.SetString("SectionDescription", model.Description ?? String.Empty);
            }
        }
        return RedirectToAction(nameof(Index));
    }

    public IActionResult SectionPage([FromRoute] String id)
    {
        Data.Entity.Section section = _dataContext.Sections.FirstOrDefault(s => s.Id.ToString() == id);
        if (section is not null)
        {
            ViewData["SectionId"] = section.Id;
            return View();
        }
        else
        {
            return NotFound();
        }
    }
}