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
    [Authorize]
    public class DocumentController : Controller
    {
        DocumentManager documentManager = new DocumentManager(new EFDocumentDal());
        FolderManager folderManager = new FolderManager(new EFFolderDal());
        private readonly IWebHostEnvironment webHostEnvironment;

        Context context = new Context();

        public DocumentController(IWebHostEnvironment webHostEnvironment)
        {
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult GetDocumentList()
        {
            var documents = documentManager.GetDocuments();

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;

            return View(documents);
        }
        [HttpGet]
        public IActionResult Upload()
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
        public async Task<IActionResult> Upload(IFormFile file, Document document)
        {
            string path;
            string oldFileName = file.FileName;
            string[] fileType = file.ContentType.Split("/");
            string newFileName = Guid.NewGuid().ToString();

            //var folderValues = context.Folders.FirstOrDefault(x => x.FolderName == "root");

            //Document newDocument = new Document();

            document.DocumentName = oldFileName;
            //newDocument.FolderID = folderValues.FolderID;
            document.Guid = newFileName;
            document.DocumentType = fileType[1];

            //documentManager.DocumentAdd(document);

            int folderID = document.FolderID;

            var folderValues = context.Folders.FirstOrDefault(x => x.FolderID == folderID);
            //if (folderValues != null)
            //    TempData["FolderPath"] = webHostEnvironment.WebRootPath + folderValues.FolderName;

            if (folderValues != null)
                try
                {
                    path = Path.Combine(Environment.CurrentDirectory, "Upload", folderValues.FolderName);
                    TempData["FolderPath"] = path;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    using (var fileStream = new FileStream(Path.Combine(path, newFileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        TempData["FileUploadSuccess"] = "File upload success.";
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("File upload failed.", ex);
                }
            documentManager.DocumentAdd(document);
            return RedirectToAction("Upload");
        }

        public IActionResult DocumentDetailsView(int id)
        {
            var documentValues = documentManager.GetByID(id);

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;

            return View(documentValues);
        }
        [HttpGet]

        public IActionResult AddDocument()
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

        public IActionResult AddDocument(Document document)
        {
            DocumentValidator validator = new DocumentValidator();
            ValidationResult results = validator.Validate(document);
            if (results.IsValid)
            {
                documentManager.DocumentAdd(document);
                return RedirectToAction("GetDocumentList");
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

        public IActionResult DeleteDocument(int id)
        {
            var documentValue = documentManager.GetByID(id);
            documentManager.DocumentRemove(documentValue);
            return RedirectToAction("GetDocumentList");
        }
        [HttpGet]

        public IActionResult DocumentUpdate(int id)
        {
            var documentValues = documentManager.GetByID(id);

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;

            return View(documentValues);
        }

        [HttpPost]

        public IActionResult DocumentUpdate(Document document)
        {
            DocumentValidator validator = new DocumentValidator();
            ValidationResult results = validator.Validate(document);
            if (results.IsValid)
            {
                documentManager.DocumentUpdate(document);
                return RedirectToAction("GetDocumentList");
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
