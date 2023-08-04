using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        RoleManager roleManager = new RoleManager(new EFRoleDal());
        FolderManager folderManager = new FolderManager(new EFFolderDal());

        Context context = new Context();

        public IActionResult GetRoles()
        {
            var roles = roleManager.GetRoles();
            return View(roles);
        }

        public IActionResult RoleDetails(int id)
        {
            var roleValues = roleManager.GetByID(id);
            var roleAuthoritiesValues = context.RoleAuthorities.FirstOrDefault(x => x.RoleID ==  roleValues.RoleID);
            if(roleAuthoritiesValues != null)
            {
                ViewBag.RoleFolder = roleAuthoritiesValues.Folder.FolderName;
            } else
            {
                ViewBag.RoleFolder = "This role has not folder.";
            }
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
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;
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
