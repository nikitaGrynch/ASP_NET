using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ASP_NET.Data;
using ASP_NET.Data.Entity;
using ASP_NET.Models.Forum;
using ASP_NET.Services.Display;
using ASP_NET.Services.Random;
using ASP_NET.Services.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace ASP_NET.Controllers;

public class ForumController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<ForumController> _logger;
    private readonly IValidationService _validationService;
    private readonly IDisplayService _displayService;
    private readonly IRandomService _randomService;
    private int Counter
    {
        get => _counter++;
        set { _counter = value; }
    }

    private int _counter;

    public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService, IDisplayService displayService, IRandomService randomService)
    {
        _dataContext = dataContext;
        _logger = logger;
        _validationService = validationService;
        _displayService = displayService;
        _randomService = randomService;
        _counter = 0;
    }

    // GET
    public IActionResult Index()
    {
        Counter = 0;
        String? userId = HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        ForumIndexModel model = new()
        {
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            Sections = _dataContext.Sections
                .Include(s => s.Author)
                .Include(s => s.RateList)
                .OrderBy(s => s.CreatedDt)
                .AsEnumerable()
                .Select(s => new ForumSectionModel()
            {
                UrlIdString = s.Id.ToString(),
                Title = s.Title,
                Description = s.Description,
                Logo = $"/imgs/logos/{(s.Logo is null ? "section" + Counter.ToString() + ".png" : s.Logo)}",
                CreatedDtString = DateTime.Today == s.CreatedDt.Date ? 
                    "Today " + s.CreatedDt.ToString("HH:mm") 
                    : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                AuthorName = s.Author.RealName,
                AuthorAvatar = s.Author.Avatar is null ? null : $"/avatars/{s.Author.Avatar}",
                AuthorLogin = s.Author.Login,
                LikesCount = s.RateList.Count(r => r.Rating > 0),
                DislikesCount = s.RateList.Count(r => r.Rating < 0),
                GivenRate = userId == null ? null
                    : s.RateList.FirstOrDefault(r => r.UserId == Guid.Parse(userId))?.Rating,
            }).ToList(),
        };
        foreach (var section in model.Sections)
        {
            section.Sights = _dataContext.Sights.AsEnumerable().Count(st => st.ItemId == Guid.Parse(section.UrlIdString));

        }
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
                Section section = new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Title = model.Title,
                    Description = model.Description,
                    CreatedDt = DateTime.Now,
                };
                if (model.Logo is not null)
                {
                    String ext = Path.GetExtension((model.Logo.FileName));
                    String name = _randomService.RandomFileName(16);
                    String logoFilename = name + ext;
                    string path = "wwwroot/imgs/logos/" + logoFilename;
                    using (var fileStream = new FileStream(
                               path,
                               FileMode.Create))
                    {
                        model.Logo.CopyTo(fileStream);
                        section.Logo = logoFilename;
                    }

                }

                _dataContext.Sections.Add(section);
                
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
        _dataContext.Sights.Add((new()
        {
            Id = Guid.NewGuid(),
            Moment = DateTime.Now,
            ItemId = Guid.Parse(id),
            UserId = null
        }));
        _dataContext.SaveChanges();
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
    
    public IActionResult Themes([FromRoute] String id)
    {
        Data.Entity.Theme? theme = null!;
        try
        {
            theme = _dataContext.Themes.Find(Guid.Parse(id));
        }
        catch { }

        if (theme is null)
        {
            return NotFound();
        }
        ForumThemesPageModel model = new()
        {
            Title = theme.Title,
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            ThemeIdString = id,
            Topics = _dataContext.Topics
                .Where(t => t.ThemeId == theme.Id)
                .Select(t => new ForumTopicViewModel()
                {
                    Title = t.Title,
                    Description = _displayService.ReduceString(t.Description, 120),
                    UrlIdString = t.Id.ToString(),
                    CreatedDtString = _displayService.DaysAgoString(t.CreatedDt),
                    AuthorName = t.Author.IsRealNamePublic ? t.Author.RealName : t.Author.Login,
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
                    Title = HttpContext.Session.GetString("TopicTitle")!,
                    Description = HttpContext.Session.GetString("TopicDescription")!
                };
                HttpContext.Session.Remove("TopicTitle");
                HttpContext.Session.Remove("TopicDescription");
            }

            HttpContext.Session.Remove("CreateMessage");
            HttpContext.Session.Remove("IsMessagePositive");
        }
        return View(model);
    }

    public IActionResult Topics([FromRoute] String id)
    {
        Data.Entity.Topic? topic = null!;
        try
        {
            topic = _dataContext.Topics.Find(Guid.Parse(id));
        }
        catch { }

        if (topic is null)
        {
            return NotFound();
        }

        ForumTopicsPageModel model = new()
        {
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            Title = topic.Title,
            Description = topic.Description,
            TopicIdString = id,
            Posts = _dataContext
                .Posts
                .Include(p => p.Author)
                .Include(p => p.Reply)
                .Where(p => p.TopicId == topic.Id)
                .Select(p => new ForumPostViewModel
                {
                    Id = p.Id.ToString(),
                    Content = p.Content,
                    CreatedDtString = _displayService.DaysAgoString(p.CreatedDt),
                    AuthorAvatar = $"/avatars/{p.Author.Avatar ?? "no-avatar.png"}",
                    AuthorName = p.Author.IsRealNamePublic ? p.Author.RealName : p.Author.Login,
                    ReplyPreview = null
                })
                .ToList()
        };
        if (HttpContext.Session.GetString("CreateMessage") is String message)
        {
            model.CreateMessage = message;
            model.IsMessagePositive =
                HttpContext.Session.GetInt32("IsMessagePositive") != 0;
            if (model.IsMessagePositive == false)
            {
                model.FormModel = new()
                {
                    Content = HttpContext.Session.GetString("PostContent")!,
                    ReplyId = HttpContext.Session.GetString("PostReply")!
                };
                HttpContext.Session.Remove("PostContent");
                HttpContext.Session.Remove("PostReply");
            }
            HttpContext.Session.Remove("CreateMessage");
            HttpContext.Session.Remove("IsMessagePositive");
        }
        
        return View(model);
    }

    [HttpPost]
    public RedirectToActionResult CreateTopic(ForumTopicFormModel model)
    {
        if (!_validationService.Validate(model.Title, ValidationTerms.NotEmpty)
            || !_validationService.Validate(model.Description, ValidationTerms.NotEmpty))
        {
            HttpContext.Session.SetString("CreateMessage" ,"Title cannot be empty");
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("TopicTitle", model.Title ?? String.Empty);
            HttpContext.Session.SetString("TopicDescription", model.Description ?? String.Empty);

        }
        else
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value!);
                _dataContext.Topics.Add(new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Title = model.Title,
                    Description = model.Description,
                    CreatedDt = DateTime.Now,
                    ThemeId = Guid.Parse(model.ThemeId)
                });
                
                _dataContext.SaveChanges();
                HttpContext.Session.SetString("CreateMessage" ,"Topic successfully created");
                HttpContext.Session.SetInt32("IsMessagePositive", 1);
            }
            catch
            {
                HttpContext.Session.SetString("CreateMessage" ,"Auth error");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("TopicTitle", model.Title ?? String.Empty);
                HttpContext.Session.SetString("TopicDescription", model.Description ?? String.Empty);
            }
        }
        return RedirectToAction(
            nameof(Themes),
            new { id = model.ThemeId }
        );
    }

    [HttpPost]
    public RedirectToActionResult CreatePost(ForumPostFormModel model)
    {
        if (!_validationService.Validate(model.Content, ValidationTerms.NotEmpty))
        {
            HttpContext.Session.SetString("CreateMessage" ,"Answer cannot be empty");
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("PostContent", model.Content ?? String.Empty);
            HttpContext.Session.SetString("PostReply", model.ReplyId ?? String.Empty);

        }
        else
        {
            Guid userId;
            try
            {
                userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value!);
                _dataContext.Posts.Add(new()
                {
                    Id = Guid.NewGuid(),
                    AuthorId = userId,
                    Content = model.Content,
                    ReplyId = String.IsNullOrEmpty(model.ReplyId) ? null : Guid.Parse(model.ReplyId),
                    CreatedDt = DateTime.Now,
                    TopicId = Guid.Parse(model.TopicId)
                });
                
                _dataContext.SaveChanges();
                HttpContext.Session.SetString("CreateMessage" ,"Answer successfully created");
                HttpContext.Session.SetInt32("IsMessagePositive", 1);
            }
            catch
            {
                HttpContext.Session.SetString("CreateMessage" ,"Answer error");
                HttpContext.Session.SetInt32("IsMessagePositive", 0);
                HttpContext.Session.SetString("PostContent", model.Content ?? String.Empty);
                HttpContext.Session.SetString("PostReply", model.ReplyId ?? String.Empty);
            }
        }
        return RedirectToAction(
            nameof(Topics),
            new {id = model.TopicId});
    }
}