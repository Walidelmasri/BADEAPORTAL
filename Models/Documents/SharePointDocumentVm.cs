namespace BADEAPORTAL.Models.Documents;

public sealed class SharePointDocumentVm
{
    public string ItemId { get; set; } = "";
    public string Name { get; set; } = "";
    public string WebUrl { get; set; } = "";
    public string FolderPath { get; set; } = "";
    public long? Size { get; set; }
    public DateTimeOffset? LastModifiedDateTime { get; set; }
    public bool IsFolder { get; set; }
}