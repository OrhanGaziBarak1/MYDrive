using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DriveUI.Controllers
{
    /*[Authorize(Roles = "Admin")]*/
    //public class DocumentTypeController : Controller
    //{
    //    DocumentTypeManager documentTypeManager = new DocumentTypeManager(new EFDocumentTypeDal());

    //    public IActionResult GetDocumentTypeList()
    //    {
    //        var documentTypes = documentTypeManager.GetDocumentTypes();
    //        return View(documentTypes);
    //    }
    //    public IActionResult DocumentTypeDetailsView(int id)
    //    {
    //        var documentTypeValues = documentTypeManager.GetByID(id);
    //        return View(documentTypeValues);
    //    }

    //    [HttpGet]
    //    public IActionResult AddDocumentType()
    //    {
    //        return View();
    //    }
    //    [HttpPost]
    //    public IActionResult AddDocumentType(DocumentType documentType)
    //    {
    //        DocumentTypeValidator validator = new DocumentTypeValidator();
    //        ValidationResult results = validator.Validate(documentType);

    //        if (results.IsValid)
    //        {
    //            documentTypeManager.DocumentTypeAdd(documentType);
    //            return RedirectToAction("GetDocumentTypeList");
    //        }
    //        else
    //        {
    //            foreach (var item in results.Errors)
    //            {
    //                ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
    //            }
    //        }
    //        return View();
    //    }

    //    public IActionResult DocumentTypeRemove(int id)
    //    {
    //        var documentTypeValue = documentTypeManager.GetByID(id);
    //        documentTypeManager.DocumentTypeRemove(documentTypeValue);
    //        return RedirectToAction("GetDocumentTypeList");
    //    }
    //    [HttpGet]
    //    public IActionResult DocumentTypeUpdate(int id)
    //    {
    //        var documentTypeValues = documentTypeManager.GetByID(id);
    //        return View(documentTypeValues);
    //    }
    //    [HttpPost]
    //    public IActionResult DocumentTypeUpdate(DocumentType documentType)
    //    {
    //        DocumentTypeValidator validator = new DocumentTypeValidator();
    //        ValidationResult results = validator.Validate(documentType);
    //        if(results.IsValid)
    //        {
    //            documentTypeManager.DocumentTypeUpdate(documentType);
    //            return RedirectToAction(nameof(GetDocumentTypeList));
    //        } else
    //        {
    //            foreach(var item in results.Errors)
    //            {
    //                ModelState.AddModelError(item.PropertyName, item.ErrorMessage);
    //            }
    //        }
    //        return View();
    //    }
    //}
}
