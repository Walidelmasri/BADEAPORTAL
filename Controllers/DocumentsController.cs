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

    public async Task<IActionResult> Index()
    {
        try
        {
            var documents = await _sharePointDocumentService.ListDocumentsAsync();

            return View(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load SharePoint documents.");

            ViewBag.ErrorMessage =
                "Documents could not be loaded right now. Please contact IT if the issue continues.";

            return View(Array.Empty<BADEAPORTAL.Models.Documents.SharePointDocumentVm>());
        }
    }
}