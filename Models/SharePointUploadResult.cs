namespace BADEAPORTAL.Models.Documents;

public sealed class SharePointUploadResult
{
    public string ItemId { get; set; } = "";

    public string FileName { get; set; } = "";

    public long? FileSize { get; set; }
}