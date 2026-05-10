using BADEAPORTAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BADEAPORTAL.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly ISharePointDocumentService _sharePointDocumentService;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(
        ISharePointDocumentService sharePointDocumentService,
        ILogger<DocumentsController> logger)
    {
        _sharePointDocumentService = sharePointDocumentService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? folderPath)
    {
        try
        {
            var documents = await _sharePointDocumentService.ListDocumentsAsync(folderPath);

            ViewBag.CurrentFolderPath = folderPath ?? "";

            return View(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SharePoint documents for folder path {FolderPath}.", folderPath);

            ViewBag.CurrentFolderPath = folderPath ?? "";
            ViewBag.ErrorMessage =
                "Documents could not be loaded right now. Please contact IT if the issue continues.";

            return View(Array.Empty<BADEAPORTAL.Models.Documents.SharePointDocumentVm>());
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
}