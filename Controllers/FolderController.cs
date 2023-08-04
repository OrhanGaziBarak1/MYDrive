using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DriveUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FolderController : Controller
    {
        FolderManager folderManager = new FolderManager(new EFFolderDal());

        public IActionResult GetFolders()
        {
            var folders = folderManager.GetFolders();
            return View(folders);
        }

        public IActionResult FolderDetails(int id)
        {
            var folderValues = folderManager.GetByID(id);
            return View(folderValues);
        }

        [HttpGet]
        public IActionResult AddFolder()
        {
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;
            return View();
        }

        [HttpPost]
        public IActionResult AddFolder(Folder folder)
        {
            FolderValidator validator = new FolderValidator();
            ValidationResult results = validator.Validate(folder);

            if (results.IsValid)
            {
                folderManager.FolderAdd(folder);
                return RedirectToAction("GetFolders");
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

        public IActionResult FolderDelete(int id)
        {
            var folderValue = folderManager.GetByID(id);
            folderManager.FolderRemove(folderValue);
            return RedirectToAction("GetFolders");
        }

        [HttpGet]
        public IActionResult FolderUpdate(int id)
        {
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;
            var folderValuesforView = folderManager.GetByID(id);
            return View(folderValuesforView);
        }
        [HttpPost]
        public IActionResult FolderUpdate(Folder folder)
        {
            FolderValidator validator = new FolderValidator();
            ValidationResult results = validator.Validate(folder);
            if (results.IsValid)
            {
                folderManager.FolderUpdate(folder);
                return RedirectToAction("GetFolders");
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
