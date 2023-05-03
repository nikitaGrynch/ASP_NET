using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ASP_NET.Data;
using ASP_NET.Models.Forum;
using ASP_NET.Services.Validation;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ASP_NET.Controllers;

public class ForumController : Controller
{
    private readonly DataContext _dataContext;
    private readonly ILogger<ForumController> _logger;
    private readonly IValidationService _validationService;

    public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
    {
        _dataContext = dataContext;
        _logger = logger;
        _validationService = validationService;
    }

    // GET
    public IActionResult Index()
    {
       
        ForumIndexModel model = new()
        {
            UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
            Sections = _dataContext.Sections.ToList()
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
}