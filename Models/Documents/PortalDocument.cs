namespace BADEAPORTAL.Models.Documents;

public sealed class PortalDocument
{
    public int DocumentId { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string? FolderPath { get; set; }

    public int Status { get; set; } = 1;

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastUpdatedBy { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    public ICollection<PortalDocumentVersion> Versions { get; set; } =
        new List<PortalDocumentVersion>();
}