using BADEAPORTAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace BADEAPORTAL.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly ISharePointDocumentService _sharePointDocumentService;
    private readonly IPortalDocumentService _portalDocumentService;
    private readonly IUserProfileService _userProfileService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ISharePointDocumentService sharePointDocumentService,
        IPortalDocumentService portalDocumentService,
        IUserProfileService userProfileService,
        ILogger<DocumentsController> logger)
    {
        _sharePointDocumentService = sharePointDocumentService;
        _portalDocumentService = portalDocumentService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? folderPath)
    {
        try
        {
            var sharePointItems = await _sharePointDocumentService
                .ListDocumentsAsync(folderPath);

            var folders = sharePointItems
                .Where(x => x.IsFolder)
                .Select(x => new BADEAPORTAL.Models.Documents.DocumentListItemVm
                {
                    Name = x.Name,
                    FolderPath = x.FolderPath,
                    Type = "Folder",
                    IsFolder = true,
                    Size = null,
                    LastModifiedDateTime = x.LastModifiedDateTime
                })
                .ToList();

            var documents = await _portalDocumentService
                .GetActiveDocumentsAsync(folderPath);

            var model = folders
                .Concat(documents)
                .OrderByDescending(x => x.IsFolder)
                .ThenBy(x => x.Name)
                .ToList();

            ViewBag.CurrentFolderPath = folderPath ?? "";

            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load documents for folder path {FolderPath}.",
                folderPath);

            ViewBag.CurrentFolderPath = folderPath ?? "";
            ViewBag.ErrorMessage =
                "Documents could not be loaded right now. Please contact IT if the issue continues.";

            return View(Array.Empty<BADEAPORTAL.Models.Documents.DocumentListItemVm>());
        }
    }

    public async Task<IActionResult> Download(string itemId)
    {
        try
        {
            var document = await _sharePointDocumentService.DownloadDocumentAsync(itemId);

            return File(document.Content, document.ContentType, document.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download SharePoint document with item id {ItemId}.", itemId);

            TempData["ErrorMessage"] =
                "The document could not be downloaded right now. Please contact IT if the issue continues.";

            return RedirectToAction(nameof(Index));
        }
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        string? folderPath,
        IFormFile? file,
        string? name,
        string? description)
    {
        try
        {
            if (file == null)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";

                return RedirectToAction(nameof(Index), new { folderPath });
            }

            var displayName = string.IsNullOrWhiteSpace(name)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : name.Trim();

            var currentUser = _userProfileService.GetCurrentUser();

            var uploadedBy =
                currentUser.EmailOrUpn ??
                currentUser.DisplayName ??
                currentUser.FullName ??
                User.Identity?.Name ??
                "Unknown";

            var uploadResult = await _sharePointDocumentService
                .UploadDocumentAsync(folderPath, file);

            await _portalDocumentService.CreateDocumentAsync(
                folderPath,
                displayName,
                description,
                file,
                uploadResult.ItemId,
                uploadedBy);

            TempData["SuccessMessage"] = "Document uploaded successfully.";

            return RedirectToAction(nameof(Index), new { folderPath });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to upload document to folder path {FolderPath}.",
                folderPath);

            TempData["ErrorMessage"] = ex.Message;

            return RedirectToAction(nameof(Index), new { folderPath });
        }
    }
}