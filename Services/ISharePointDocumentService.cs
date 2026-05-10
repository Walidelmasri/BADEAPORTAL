using BADEAPORTAL.Models.Documents;

namespace BADEAPORTAL.Services;

public interface ISharePointDocumentService
{
    Task<IReadOnlyList<SharePointDocumentVm>> ListDocumentsAsync();
}