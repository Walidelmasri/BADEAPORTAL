using BADEAPORTAL.Models.Documents;

namespace BADEAPORTAL.Services;

public interface ISharePointDocumentService
{
    Task<IReadOnlyList<SharePointDocumentVm>> ListDocumentsAsync(string? folderPath);

    Task<(Stream Content, string FileName, string ContentType)> DownloadDocumentAsync(string itemId);
}