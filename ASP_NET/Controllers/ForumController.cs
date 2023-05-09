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
                UrlIdString = s.Id.ToString(),
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

    public IActionResult Sections([FromRoute] String id)
    {
        ViewData["SectionId"] = id;
        ForumSectionsModel model = new()
        {
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            Themes = _dataContext.Themes
                .Include(t => t.Author)
                .Where(t => t.SectionId == Guid.Parse(id))
                .Select(t => new ForumThemeModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    UrlIdString = t.Id.ToString(),
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date ? 
                        "Today " + t.CreatedDt.ToString("HH:mm") 
                        : t.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    AuthorAvatar = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                    AuthorName = t.Author.IsRealNamePublic ? t.Author.RealName : t.Author.Login,
                    CreatedDt = t.CreatedDt,
                    AuthorRegisterDtString = t.Author.RegisterDt.Date.ToString()
                })
                .ToList()
        };
        if (HttpContext.Session.GetString("CreateMessage") is String message)
        {
            model.CreateMessage = message;
            model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") != 0;

            if (model.IsMessagePositive == false)
            {
                model.FormModel = new()
                {
                    Title = HttpContext.Session.GetString("ThemeTitle")!,
                    Description = HttpContext.Session.GetString("ThemeDescription")!
                };
                HttpContext.Session.Remove("ThemeTitle");
                HttpContext.Session.Remove("ThemeDescription");
            }

            HttpContext.Session.Remove("CreateMessage");
            HttpContext.Session.Remove("IsMessagePositive");
        }
        return View(model);
    }

    [HttpPost]
    public RedirectToActionResult CreateTheme([FromForm] ForumThemeFormModel model)
    {
        _logger.LogInformation("Title: {t}, Description: {d}", model.Title, model.Description);
        if (!_validationService.Validate(model.Title, ValidationTerms.NotEmpty)
            || !_validationService.Validate(model.Description, ValidationTerms.NotEmpty))
        {
            HttpContext.Session.SetString("CreateMessage" ,"Title cannot be empty");
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("ThemeTitle", model.Title ?? String.Empty);
            HttpContext.Session.SetString("ThemeDescription", model.Description ?? String.Empty);

        }
        else
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value!);
                _dataContext.Themes.Add(new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Title = model.Title,
                    Description = model.Description,
                    CreatedDt = DateTime.Now,
                    SectionId = Guid.Parse(model.SectionId)
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
        return RedirectToAction(nameof(Sections), new{id = model.SectionId});
    }
}