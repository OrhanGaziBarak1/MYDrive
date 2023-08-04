using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserAuthorityController : Controller
    {
        UserAuthorityManager userAuthorityManager = new UserAuthorityManager(new EFUserAuthorityDal());
        UserManager userManager = new UserManager(new EFUserDal());
        RoleManager roleManager = new RoleManager(new EFRoleDal());

        public IActionResult GetUserAuthorities()
        {
            var userAuthorities = userAuthorityManager.GetUserAuthorities();
            return View(userAuthorities);
        }

        public IActionResult UserAuthorityDetails(int id)
        {
            var userAuthorityValues = userAuthorityManager.GetByID(id);
            return View(userAuthorityValues);
        }


        [HttpGet]
        public IActionResult AddUserAuthority()
        {
            List<SelectListItem> userValues = (from x in userManager.GetUsers()
                                               select new SelectListItem
                                               {
                                                   Text = x.UserName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            ViewBag.Users = userValues;
            ViewBag.Roles = roleValues;
            return View();
        }
        [HttpPost]
        public IActionResult AddUserAuthority(UserAuthority userAuthority)
        {
            UserAuthorityValidator validator = new UserAuthorityValidator();
            ValidationResult results = validator.Validate(userAuthority);
            if (results.IsValid)
            {
                userAuthorityManager.AddUserAuthority(userAuthority);
                return RedirectToAction("GetUserAuthorities");
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
        public IActionResult RemoveUserAuthoirty(int id)
        {
            var userAuthorityValues = userAuthorityManager.GetByID(id);
            userAuthorityManager.UserAuthorityRemove(userAuthorityValues);
            return RedirectToAction("GetUserAuthorities");
        }
        [HttpGet]
        public IActionResult UserAuthorityUpdate(int id)
        {
            List<SelectListItem> userValues = (from x in userManager.GetUsers()
                                               select new SelectListItem
                                               {
                                                   Text = x.UserName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            ViewBag.Users = userValues;
            ViewBag.Roles = roleValues;
            var userAuthorityValues = userAuthorityManager.GetByID(id);
            return View(userAuthorityValues);
        }
        [HttpPost]
        public IActionResult UserAuthorityUpdate(UserAuthority userAuthority)
        {
            UserAuthorityValidator validator = new UserAuthorityValidator();
            ValidationResult results = validator.Validate(userAuthority);
            if (results.IsValid)
            {
                userAuthorityManager.UserAuthorityUpdate(userAuthority);
                return RedirectToAction("GetUserAuthorities");
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
