using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleAuthorityController : Controller
    {
        RoleAuthorityManager roleAuthorityManager = new RoleAuthorityManager(new EFRoleAuthorityDal());
        RoleManager roleManager = new RoleManager(new EFRoleDal());
        FolderManager folderManager = new FolderManager(new EFFolderDal());
        
        public IActionResult GetRoleAuthorities()
        {
            var roleAuthorities = roleAuthorityManager.GetRoleAuthorities();
            return View(roleAuthorities);
        }

        public IActionResult RoleAuthorityDetails(int id)
        {
            var roleAuthorityValues = roleAuthorityManager.GetByID(id);
            return View(roleAuthorityValues);
        }

        [HttpGet]
        public IActionResult AddRoleAuthorities()
        {
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                               select new SelectListItem
                                               {
                                                   Text = x.FolderName,
                                                   Value = x.FolderID.ToString()
                                               }).ToList();
            ViewBag.Roles = roleValues;
            ViewBag.Folders = folderValues;

            return View();
        }

        [HttpPost]
        public IActionResult AddRoleAuthorities(RoleAuthority roleAuthority)
        {
            RoleAuthorityValidator validator = new RoleAuthorityValidator();
            ValidationResult results = validator.Validate(roleAuthority);
            if(results.IsValid)
            {
            roleAuthorityManager.AddRoleAuthortiy(roleAuthority);
            return RedirectToAction("GetRoleAuthorities");
            } else
            {
                foreach(var item in results.Errors)
                {
                    ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
                }
            }
            return View();
        }
        public IActionResult RemoveRoleAuthority(int id)
        {
            var roleAuthorityValues = roleAuthorityManager.GetByID(id);
            roleAuthorityManager.RoleAuthorityRemove(roleAuthorityValues);
            return RedirectToAction("GetRoleAuthorities");
        }

        [HttpGet]
        public IActionResult RoleAuthorityUpdate(int id)
        {
            List<SelectListItem> roleValues = (from x in roleManager.GetRoles()
                                               select new SelectListItem
                                               {
                                                   Text = x.RoleName,
                                                   Value = x.RoleID.ToString()
                                               }).ToList();
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Roles = roleValues;
            ViewBag.Folders = folderValues;
            var roleAuthorityValues = roleAuthorityManager.GetByID(id);
            return View(roleAuthorityValues);
        }

        [HttpPost]
        public IActionResult RoleAuthorityUpdate(RoleAuthority roleAuthority)
        {
            RoleAuthorityValidator validator = new RoleAuthorityValidator();
            ValidationResult results = validator.Validate(roleAuthority);
            if (results.IsValid)
            {
                roleAuthorityManager.RoleAuthorityUpdate(roleAuthority);
                return RedirectToAction("GetRoleAuthorities");
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
