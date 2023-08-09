using BusinessLayer.Concrete;
using BusinessLayer.ValidationRules;
using DataAccessLayer.Concrete;
using DataAccessLayer.EntityFramework;
using EntityLayer.Concrete;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data.Entity;

namespace DriveUI.Controllers
{
    [Authorize]
    public class DocumentController : Controller
    {
        DocumentManager documentManager = new DocumentManager(new EFDocumentDal());
        FolderManager folderManager = new FolderManager(new EFFolderDal());
        private readonly IWebHostEnvironment? webHostEnvironment;
        public readonly IHttpContextAccessor? accessor;

        Context context = new Context();

        public DocumentController(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor accessor)
        {
            this.webHostEnvironment = webHostEnvironment;
            this.accessor = accessor;
        }

        public async Task<IActionResult> GetDocumentList(string searchTerm)
        {
            var documents = from doc in context.Documents select doc;

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = x.FolderName,
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                documents = documents.Where(s => s.DocumentName.Contains(searchTerm));
            }
            ViewBag.Folders = folderValues;
            return View(await documents.ToListAsync());
        }

        public async Task<IActionResult> GetDocumentListByRole(int id, string searchTerm)
        {
            var user = context.Users.FirstOrDefault(x => x.UserID == id);
            string userRole = "";
            if (user != null)
                userRole += user.Role.RoleName;
            var documentsByRole = context.Documents.Where(x => x.Role.RoleName == userRole);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                documentsByRole = documentsByRole.Where(s => s.DocumentName.Contains(searchTerm));
            }
            return View(await documentsByRole.ToListAsync());
        }

        [HttpGet]
        public IActionResult Upload()
        {
            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 where x.FolderName != "root"
                                                 select new SelectListItem
                                                 {
                                                     Text = getFoldersPath(x),
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, Document document)
        {
            string absolutePath;
            string[] splitedAbsolutePath;
            string folderPath = "";
            string oldFileName = file.FileName;
            string fileType = file.ContentType;
            string newFileName = Guid.NewGuid().ToString();
            string folderRole;

            document.DocumentName = oldFileName;
            document.Guid = newFileName;
            document.DocumentType = fileType;

            int folderID = document.FolderID;

            var folderValues = context.Folders.FirstOrDefault(x => x.FolderID == folderID);

            if (folderValues != null)
                folderPath = getFoldersPath(folderValues);

            absolutePath = Path.Combine("wwwroot\\Upload\\", folderPath);

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            using (var fileStream = new FileStream(Path.Combine(absolutePath, newFileName), FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                TempData["FileUploadSuccess"] = "File upload success.";
            }

            splitedAbsolutePath = absolutePath.Split("\\");

            folderRole = splitedAbsolutePath[3];

            var role = context.Roles.FirstOrDefault(x => x.RoleName == folderRole);
            if (role != null)
                document.RoleID = role.RoleID;

            documentManager.DocumentAdd(document);

            if (accessor.HttpContext.Session.GetString("UserRole") != null && accessor.HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("GetDocumentList");
            }
            else
            {
                return RedirectToAction("GetDocumentListByRole");
            }

        }

        public IActionResult DocumentDetailsView(int id)
        {
            var documentValues = documentManager.GetByID(id);

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 select new SelectListItem
                                                 {
                                                     Text = getFoldersPath(x),
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
            string folderPath = "";
            string filePath = "";

            var documentValue = documentManager.GetByID(id);

            int folderID = documentValue.FolderID;

            var folderValues = context.Folders.FirstOrDefault(x => x.FolderID == folderID);

            if (folderValues != null)
                folderPath = getFoldersPath(folderValues);

            filePath += Path.Combine(folderPath, documentValue.Guid);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            documentManager.DocumentRemove(documentValue);
            return RedirectToAction("GetDocumentList");
        }
        [HttpGet]
        public IActionResult DocumentUpdate(int id)
        {
            var documentValues = documentManager.GetByID(id);

            List<SelectListItem> folderValues = (from x in folderManager.GetFolders()
                                                 where x.FolderName != "root"
                                                 select new SelectListItem
                                                 {
                                                     Text = getFoldersPath(x),
                                                     Value = x.FolderID.ToString()
                                                 }).ToList();
            ViewBag.Folders = folderValues;

            return View(documentValues);
        }

        [HttpPost]
        public IActionResult DocumentUpdate(Document document)
        {
            string sourceFolderPath = "wwwroot\\Upload\\";
            string sourceAbsolutePath = "";

            string destinationFolderPath = "wwwroot\\Upload\\";
            string destinationAbsolutePath = "";
            string[] splitedDestinationAbsolutePath;
            string newDocumentRole;

            var documentNowValues = context.Documents.FirstOrDefault(x => x.DocumentID == document.DocumentID);
            var nowFolder = context.Folders.FirstOrDefault(x => x.FolderID == documentNowValues.FolderID);

            if (documentNowValues != null)
            {
                if (nowFolder != null)
                    sourceFolderPath += getFoldersPath(nowFolder);

                sourceAbsolutePath = Path.Combine(sourceFolderPath, documentNowValues.Guid);
            }

            var destinationFolder = context.Folders.FirstOrDefault(x => x.FolderID == document.FolderID);

            if (destinationFolder != null)
                destinationFolderPath += getFoldersPath(destinationFolder);

            if (!Directory.Exists(destinationFolderPath))
            {
                Directory.CreateDirectory(destinationFolderPath);

            }

            destinationAbsolutePath = Path.Combine(destinationFolderPath, document.Guid);

            splitedDestinationAbsolutePath = destinationAbsolutePath.Split("\\");
            newDocumentRole = splitedDestinationAbsolutePath[3];

            var newRole = context.Roles.FirstOrDefault(x => x.RoleName == newDocumentRole);

            if (System.IO.File.Exists(sourceAbsolutePath))
            {
                System.IO.File.Copy(sourceAbsolutePath, destinationAbsolutePath);
                System.IO.File.Delete(sourceAbsolutePath);
            }

            if (newRole != null)
                document.RoleID = newRole.RoleID;

            documentManager.DocumentUpdate(document);

            if (accessor.HttpContext.Session.GetString("UserRole") != null && accessor.HttpContext.Session.GetString("UserRole") == "Admin")
            {
                return RedirectToAction("GetDocumentList");
            }
            else
            {
                return RedirectToAction("GetDocumentListByRole");
            }

        }

        public IActionResult DownloadFile(int id)
        {
            string documentPath = "";
            var document = context.Documents.FirstOrDefault(doc => doc.DocumentID == id);

            if (document != null)
            {
                var folder = context.Folders.FirstOrDefault(folder => folder.FolderID == document.FolderID);
                if (folder != null)
                    documentPath += Path.Combine("wwwroot\\Upload\\", folder.FolderName, document.Guid);
            }

            if (document != null && documentPath != null)
            {
                return File(System.IO.File.ReadAllBytes(documentPath), document.DocumentType, document.DocumentName);
            }
            else
            {
                return Problem("Something happened... :'(");
            }

        }

        public string getFoldersPath(Folder folder)
        {
            var folders = new List<string>();
            var currentFolder = folder;
            string path = "root\\";

            while (currentFolder.FolderID != 1 && currentFolder.FolderName != "root")
            {
                folders.Add(currentFolder.FolderName);
                currentFolder = folderManager.GetByID(currentFolder.RootFolderID);
            }

            for (int loop = folders.Count - 1; loop >= 0; loop--)
            {
                path += folders[loop] + "\\";
            }
            return path;
        }
    }
}
