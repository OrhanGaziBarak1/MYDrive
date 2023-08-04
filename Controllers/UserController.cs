using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        UserManager userManager = new UserManager(new EFUserDal());
        RoleManager roleManager = new RoleManager(new EFRoleDal());

        Context context = new Context();


        public IActionResult GetUsers()
        {
            var users = userManager.GetUsers();
            return View(users);
        }


        public IActionResult UserDetails(int id)
        {
            var userValues = userManager.GetByID(id);
            var userAuthorityValues = context.UserAuthorities.FirstOrDefault(x => x.UserID == userValues.UserID);
            ViewBag.UserAuthorityValues = userAuthorityValues;
            return View(userValues);
        }


        [HttpGet]
        public IActionResult AddUser()
        {
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            ViewBag.Roles = roleValues;
            return View();
        }

        [HttpPost]
        public IActionResult AddUser(User user)
        {
            UserValidator validator = new UserValidator();
            ValidationResult results = validator.Validate(user);

            if (results.IsValid)
            {
                userManager.AddUser(user);
                return RedirectToAction("GetUsers");
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

        public IActionResult RemoveUser(int id)
        {
            var userValues = userManager.GetByID(id);
            userManager.DeleteUser(userValues);
            return RedirectToAction("GetUsers");
        }

        [HttpGet]
        public IActionResult UserUpdate(int id)
        {
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            ViewBag.Roles = roleValues;
            var userValues = userManager.GetByID(id);
            return View(userValues);
        }
        [HttpPost]
        public IActionResult UserUpdate(User user)
        {
            UserValidator validator = new UserValidator();
            ValidationResult results = validator.Validate(user);

            if (results.IsValid)
            {
                userManager.UserUpdate(user);
                return RedirectToAction("GetUsers");
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
