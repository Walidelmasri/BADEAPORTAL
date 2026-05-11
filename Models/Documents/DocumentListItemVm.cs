namespace BADEAPORTAL.Models.Documents;

public sealed class DocumentListItemVm
{
    public int? DocumentId { get; set; }

    public string? SharePointItemId { get; set; }

    public string Name { get; set; } = "";

    public string? Description { get; set; }

    public string Type { get; set; } = "";

    public string FolderPath { get; set; } = "";

    public long? Size { get; set; }

    public DateTimeOffset? LastModifiedDateTime { get; set; }

    public bool IsFolder { get; set; }
}