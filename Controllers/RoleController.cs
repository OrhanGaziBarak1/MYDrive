using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        RoleManager roleManager = new RoleManager(new EFRoleDal());

        public IActionResult GetRoles()
        {
            var roles = roleManager.GetRoles();
            return View(roles);
        }

        public IActionResult RoleDetails(int id)
        {
            var roleValues = roleManager.GetByID(id);
            return View(roleValues);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddRole(Role role)
        {
            RoleValidator validator = new RoleValidator();
            ValidationResult results = validator.Validate(role);

            if (results.IsValid)
            {
                roleManager.AddRole(role);
                return RedirectToAction("GetRoles");
            } else
            {
                foreach(var item in results.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }
            return View();
        }
        public IActionResult RemoveRole(int id) 
        {
            var roleValues = roleManager.GetByID(id);
            roleManager.RoleRemove(roleValues);
            return RedirectToAction("GetRoles");
        }
        [HttpGet]
        public IActionResult RoleUpdate(int id)
        {
            var roleValues = roleManager.GetByID(id);
            return View(roleValues);
        }

        [HttpPost]
        public IActionResult RoleUpdate(Role role)
        {
            RoleValidator validator = new RoleValidator();
            ValidationResult results = validator.Validate(role);

            if (results.IsValid)
            {
                roleManager.RoleUpdate(role);
                return RedirectToAction("GetRoles");
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
