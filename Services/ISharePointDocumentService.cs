using BADEAPORTAL.Models.Documents;
using Microsoft.AspNetCore.Http;

namespace BADEAPORTAL.Services;

public interface ISharePointDocumentService
{
    Task<IReadOnlyList<SharePointDocumentVm>> ListDocumentsAsync(string? folderPath);

    Task<(Stream Content, string FileName, string ContentType)> DownloadDocumentAsync(string itemId);

    Task UploadDocumentAsync(string? folderPath, IFormFile file);
}