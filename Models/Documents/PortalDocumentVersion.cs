namespace BADEAPORTAL.Models.Documents;

public sealed class PortalDocumentVersion
{
    public int VersionId { get; set; }

    public int DocumentId { get; set; }

    public int VersionNo { get; set; }

    public string OriginalFileName { get; set; } = "";

    public string? FileType { get; set; }

    public string SharePointItemId { get; set; } = "";

    public long? FileSize { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public int IsCurrent { get; set; } = 1;

    public string? FilePath { get; set; }

    public PortalDocument? Document { get; set; }
}