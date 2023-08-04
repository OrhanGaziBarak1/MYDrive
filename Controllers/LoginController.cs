using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using DriveUI.Models;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DriveUI.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        public readonly IHttpContextAccessor accessor;

        LoginManager loginManager = new LoginManager();
        UserManager userManager = new UserManager(new EFUserDal());
        Context context = new Context();

        public LoginController(IHttpContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            bool isLogin = loginManager.Login(user);
            if (isLogin)
            {

                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, loginManager.isUser.UserName),
                        new Claim(ClaimTypes.Role, loginManager.isUser.Role.RoleName),

                    };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30),
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["loginInformationsHome"] = claims[0].Value;
                TempData["userRole"] = claims[1].Value;

                if (accessor.HttpContext != null)
                {
                    accessor.HttpContext.Session.SetString("UserName", claims[0].Value);
                    accessor.HttpContext.Session.SetString("UserRole", claims[1].Value);
                    accessor.HttpContext.Session.SetInt32("UserID", loginManager.isUser.UserID);
                }

                if (claims[1].Value != "No Role")
                {
                    return RedirectToAction("Index", "Home");
                } else
                {
                    return RedirectToAction("NoRoleHome", "Home");
                }
 
            }
            else
            {
                TempData["loginInformationsLogin"] = "You can not login the system. Please control your mail address and password or register.";
                return View();
            }
        }
        public async Task<IActionResult> Logout()
        {
            if (accessor.HttpContext.Session != null)
            {
                accessor.HttpContext.Session.Clear();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["logout"] = "You logged out.";
            return RedirectToAction("Login");

        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Register(User user)
        {
            UserValidator validator = new UserValidator();

            var role = context.Roles.FirstOrDefault(x => x.RoleName.Equals("No role"));

            if (role != null)
            {
                user.RoleID = role.RoleID;
            }

            ValidationResult results = validator.Validate(user);

            if (results.IsValid)
            {
                userManager.AddUser(user);
                TempData["registerSuccess"] = "You registered successfully.";
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var item in results.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }
            return View();
        }
        [HttpGet]
        public IActionResult MyProfile(int id)
        {
            var profile = userManager.GetByID(id);
            return View(profile);
        }
        [HttpPost]
        public IActionResult MyProfile(User user)
        {
            UserValidator validator = new UserValidator();
            ValidationResult results = validator.Validate(user);

            if (results.IsValid)
            {
                userManager.UserUpdate(user);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var item in results.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }
            return View();
        }
    }
}
