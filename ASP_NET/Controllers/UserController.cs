using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ASP_NET.Models.User;
using ASP_NET.Services.Hash;
using Microsoft.AspNetCore.Mvc;

namespace ASP_NET.Controllers
{
    // [Route("Users")]
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;

        public UserController(IHashService hashService, ILogger<UserController> logger)
        {
            _hashService = hashService;
            _logger = logger;
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
            if (String.IsNullOrEmpty(userRegistrationModel.Login))
            {
                validationResult.LoginMessage = "Login can not be empty";
                isModelValid = false;
            }
            #endregion
            #region Password / Repeat Validation
            if (String.IsNullOrEmpty(userRegistrationModel.Password))
            {
                validationResult.PasswordMessage = "Password can not be empty";
                isModelValid = false;
            }
            else
            {
                if (userRegistrationModel.Password.Length < minPasswordLength)
                {
                    validationResult.PasswordMessage = $"Password length must be at least {minPasswordLength} characters";
                    isModelValid = false;
                }

                if (!userRegistrationModel.Password.Equals(userRegistrationModel.RepeatPassword))
                {
                    validationResult.RepeatPasswordMessage = "Passwords must be equal";
                    isModelValid = false;
                }
            }
            #endregion

            #region Email Validation
            if (String.IsNullOrEmpty(userRegistrationModel.Email))
            {
                validationResult.EmailMessage = "Email can not be empty";
                isModelValid = false;
            }
            else
            {
                String emailRegex = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,})+)$";
                if (!Regex.IsMatch(userRegistrationModel.Email, emailRegex, RegexOptions.IgnoreCase))
                {
                    validationResult.EmailMessage = "Email is incorrect";
                    isModelValid = false;
                }
            }

            #endregion

            #region Real Name Validation

            if (String.IsNullOrEmpty(userRegistrationModel.RealName))
            {
                validationResult.RealNameMessage = "Real name can not be empty";
                isModelValid = false;
            }
            else
            {
                String nameRegex = @"^.+$";
                if (!Regex.IsMatch(userRegistrationModel.Email, nameRegex, RegexOptions.IgnoreCase))
                {
                    validationResult.RealNameMessage = "Real name is incorrect";
                    isModelValid = false;
                }
            }

            #endregion

            #region IsAgree Validation

            if (userRegistrationModel.IsAgree == false)
            {
                validationResult.IsAgreeMessage = "You can't end registration without accepting the terms";
            }

            #endregion

            #region Avatar Uploading

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
                    String hash = _hashService.Hash((userRegistrationModel.Avatar.FileName + Guid.NewGuid()))[..16];
                    string path = "wwwroot/avatars/" + hash + ext;

                    // check if the file with this name already exists
                    DirectoryInfo dir = new(path);
                    FileInfo[] files = dir.GetFiles();
                    bool isWrongFile = false;
                    foreach (var file in files)
                    {
                        if (file.Name.Equals(hash))
                        {
                            validationResult.AvatarMessage = "Something was wrong. Try again, please";
                            isModelValid = false;
                            isWrongFile = false;
                            break;
                        }
                    }

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
                return View(userRegistrationModel);
            }
            else
            {

                ViewData["validationResult"] = validationResult;
                ViewData["userRegistrationModel"] = userRegistrationModel;
                return View("Registration");
            }
        }
    }
}