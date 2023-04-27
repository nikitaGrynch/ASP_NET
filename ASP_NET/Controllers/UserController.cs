using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASP_NET.Data;
using ASP_NET.Data.Entity;
using ASP_NET.Models;
using ASP_NET.Models.User;
using ASP_NET.Services.Email;
using ASP_NET.Services.Hash;
using ASP_NET.Services.Kdf;
using ASP_NET.Services.Random;
using ASP_NET.Services.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace ASP_NET.Controllers
{
    // [Route("Users")]
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;
        private readonly IValidationService _validationService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext,
            IRandomService randomService, IKdfService kdfService, IValidationService validationService, IConfiguration configuration, IEmailService emailService)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _kdfService = kdfService;
            _validationService = validationService;
            _configuration = configuration;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Registration()
        {
            return View();
        }

        public IActionResult Registration_HW()
        {
            return View();
        }

        public IActionResult RegisterUser(UserRegistrationModel userRegistrationModel)
        {
            UserValidationModel validationResult = new();
            bool isModelValid = true;
            byte minPasswordLength = 3;

            #region Login Validation

            if (!_validationService.Validate(userRegistrationModel.Login, ValidationTerms.NotEmpty))
            {
                validationResult.LoginMessage = "Login can not be empty";
                isModelValid = false;
            }

            else if (_dataContext.Users.Any(u => u.Login.ToLower() == userRegistrationModel.Login.ToLower()))
            {
                validationResult.LoginMessage = $"Login '{userRegistrationModel.Login}' is already in use";
                isModelValid = false;
            }

            #endregion

            #region Password / Repeat Validation

            if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.NotEmpty))
            {
                validationResult.PasswordMessage = "Password can not be empty";
                isModelValid = false;
            }
            else if (!_validationService.Validate(userRegistrationModel.Password, ValidationTerms.Password))
            {
                validationResult.PasswordMessage =
                    $"Password length must be at least 3 characters";
                isModelValid = false;
            }
            else if (!userRegistrationModel.Password.Equals(userRegistrationModel.RepeatPassword))
            {
                validationResult.RepeatPasswordMessage = "Passwords must be equal";
                isModelValid = false;
            }

            #endregion

            #region Email Validation

            if (!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.NotEmpty))
            {
                validationResult.EmailMessage = "Email can not be empty";
                isModelValid = false;
            }
            else if(!_validationService.Validate(userRegistrationModel.Email, ValidationTerms.Email))
            {
                validationResult.EmailMessage = "Email is incorrect";
                isModelValid = false;
            }

            #endregion

            #region Real Name Validation

            if (!_validationService.Validate(userRegistrationModel.RealName, ValidationTerms.NotEmpty))
            {
                validationResult.RealNameMessage = "Real name can not be empty";
                isModelValid = false;
            }
            else if(_validationService.Validate(userRegistrationModel.RealName, ValidationTerms.RealName))
            { 
                validationResult.RealNameMessage = "Real name is incorrect";
                isModelValid = false;
            }

            #endregion

            #region IsAgree Validation

            if (userRegistrationModel.IsAgree == false)
            {
                validationResult.IsAgreeMessage = "You can't end registration without accepting the terms";
            }

            #endregion

            #region Avatar Uploading

            string? avatarFilename = null;

            if (userRegistrationModel.Avatar is not null)
            {
                if (userRegistrationModel.Avatar.Length <= 1000)
                {
                    validationResult.AvatarMessage = "Too small file. File must be larger than 1Kb";
                    isModelValid = false;
                }
                else
                {
                    String ext = Path.GetExtension((userRegistrationModel.Avatar.FileName));
                    String name = _randomService.RandomFileName(16);
                    avatarFilename = name + ext;
                    string path = "wwwroot/avatars/" + avatarFilename;


                    // check if the file with this name already exists
                    bool isWrongFile = System.IO.File.Exists(path);
                    ViewData["avatarFilename"] = avatarFilename;

                    if (!isWrongFile)
                    {
                        using (var fileStream = new FileStream(
                                   path,
                                   FileMode.Create))
                        {
                            userRegistrationModel.Avatar.CopyTo(fileStream);

                        }
                    }
                }
            }

            #endregion

            if (isModelValid)
            {
                String passSault = _randomService.RandomString(8);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = userRegistrationModel.Login,
                    PasswordSalt = passSault,
                    PasswordHash = _kdfService.GetDerivedKey(userRegistrationModel.Password, passSault),
                    Avatar = avatarFilename,
                    Email = userRegistrationModel.Email,
                    RealName = userRegistrationModel.RealName,
                    RegisterDt = DateTime.Now,
                    LastEnterDt = null,
                    EmailCode = _randomService.ConfirmCode(6)
                };
                _dataContext.Users.Add(user);
                _dataContext.SaveChanges();
                
                // send email confirmation code
                try
                {
                    _emailService.Send("confirm_email", new Models.Email.ConfirmEmailModel()
                    {
                        Email = user.Email,
                        EmailCode = user.EmailCode,
                        RealName = user.RealName,
                        ConfirmUrl = "#"
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"_emailService error '{ex}'", ex.Message);
                }
                
                return View(userRegistrationModel);
            }
            else
            {

                ViewData["validationResult"] = validationResult;
                ViewData["userRegistrationModel"] = userRegistrationModel;
                return View("Registration");
            }
        }

        [HttpPost] // метод доступный только POST запросом
        public String AuthUser()
        {
            var loginValues = Request.Form["user-login"];
            if (loginValues.Count == 0)
            {
                return "No login data";
            }

            String login = loginValues[0] ?? "";
            var passwordValues = Request.Form["user-password"];
            if (passwordValues.Count == 0)
            {
                return "No password data";
            }

            String password = passwordValues[0] ?? "";
            User? user = _dataContext.Users.FirstOrDefault(u => u.Login == login);
            if (user is not null)
            {
                if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
                {
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return "OK";
                }
            }

            return "REJECTED";
        }

        public IActionResult Logout()
        {
            if (HttpContext.Items.Keys.Contains("authUser"))
            {
                HttpContext.Items.Remove("authUser");
            }

            if (HttpContext.Session.Keys.Contains("authUserId"))
            {
                HttpContext.Session.Remove("authUserId");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] String code)
        {
            StatusDataModel model = new();
            if (String.IsNullOrEmpty(code))
            {
                model.Status = "400";
                model.Data = "Missing required param: code";
            }
            else if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                model.Status = "401";
                model.Data = "Authenticated";
            }
            else
            {
                User? user = null;
                try
                {
                    user = _dataContext.Users
                        .Find(Guid.Parse(
                            HttpContext.User.Claims
                                .First((claim) => claim.Type == ClaimTypes.Sid).Value
                        ));
                }
                catch { }

                if (user is null)
                {
                    model.Status = "403";
                    model.Data = "Unauthorized";
                }
                else if (user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already confirmed";
                }
                else if (user.EmailCode != code)
                {
                    model.Status = "406";
                    model.Data = "Code Not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model);
        }

        public IActionResult Profile([FromRoute] String id)
        {
            //_logger.LogInformation(id);
            Data.Entity.User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                Models.User.ProfileModel model = new(user);
                if (String.IsNullOrEmpty(model.Avatar))
                {
                    model.Avatar = "no-avatar.png";
                }

                // check, is user is authorized and login belongs to them
                String userLogin =
                    HttpContext.User.Claims
                        .First(claim => claim.Type == ClaimTypes.NameIdentifier)
                        .Value;
                if (model.Login == userLogin)
                {
                    model.IsPersonal = true;
                }

                return View(model);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        public JsonResult Update([FromBody] UserUpdateModel model)
        {
            if (model is null)
            {
                return Json(new
                {
                    status = "Error", data = "No or invalid data"
                });
            }

            if (HttpContext.User.Identity?.IsAuthenticated != true)
            {
                return Json(new
                {
                    status = "Error", data = "Unauthenticated"
                });
            }

            User? user = null;
            try
            {
                user = _dataContext.Users
                    .Find(Guid.Parse(
                        HttpContext.User.Claims
                        .First((claim) => claim.Type == ClaimTypes.Sid).Value
                    ));
            }
            catch { }

            if (user is null)
            {
                return Json(new
                {
                    status = "Error", data = "Unauthorized"
                });
            }
            switch (model.Field)
            {
                case "realname":
                {
                    if (!_validationService.Validate(model.Value, ValidationTerms.RealName))
                    {
                        return Json(new
                        {
                            status = "Error",
                            data = $"Validation fails for field '{model.Field}', value = '{model.Value}'"
                        });
                    }
                    user.RealName = model.Value;
                    _dataContext.SaveChanges();
                    return Json(new
                    {
                        status = "OK", data = $"Name changed to {user.RealName}"
                    });
                }
                case "email":
                {
                    if (!_validationService.Validate(model.Value, ValidationTerms.Email))
                    {
                        return Json(new
                        {
                            status = "Error",
                            data = $"Validation fails for field '{model.Field}', value = '{model.Value}'"
                        });
                    }
                    user.Email = model.Value;
                    _dataContext.SaveChanges();
                    return Json(new
                    {
                        status = "OK", data = $"Email changed to {user.Email}"
                    });
                }
                default:
                {
                    return Json(new
                    {
                        status = "Error", data = $"Unknown field {model.Field}"
                    });
                }
            }
        }
    }
}