using BADEAPORTAL.Models.Documents;
using Microsoft.AspNetCore.Http;

namespace BADEAPORTAL.Services;

public interface IPortalDocumentService
{
Task<IReadOnlyList<DocumentListItemVm>> GetActiveDocumentsAsync(string? folderPath);
    Task CreateDocumentAsync(
        string? folderPath,
        string name,
        string? description,
        IFormFile file,
        string sharePointItemId,
        string uploadedBy);

    Task CreateNewVersionAsync(
        int documentId,
        IFormFile file,
        string sharePointItemId,
        string uploadedBy);

    Task<IReadOnlyList<PortalDocumentVersion>> GetVersionHistoryAsync(int documentId);

    Task DeactivateDocumentAsync(int documentId, string updatedBy);
}