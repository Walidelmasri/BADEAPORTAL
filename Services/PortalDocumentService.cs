using BADEAPORTAL.Data;
using BADEAPORTAL.Models.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Services;

public sealed class PortalDocumentService : IPortalDocumentService
{
    private readonly PortalDbContext _dbContext;

    public PortalDocumentService(PortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PortalDocument>> GetActiveDocumentsAsync(
        string? folderPath)
    {
        var cleanFolderPath = NormalizeFolderPath(folderPath);

        return await _dbContext.PortalDocuments
            .AsNoTracking()
            .Include(x => x.Versions.Where(v => v.IsCurrent == 1))
            .Where(x =>
                x.Status == 1 &&
                (x.FolderPath ?? "") == cleanFolderPath)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task CreateDocumentAsync(
        string? folderPath,
        string name,
        string? description,
        IFormFile file,
        string sharePointItemId,
        string uploadedBy)
    {
        var cleanFolderPath = NormalizeFolderPath(folderPath);

        var document = new PortalDocument
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            FolderPath = cleanFolderPath,
            Status = 1,
            CreatedBy = uploadedBy,
            CreatedAt = GetRiyadhNow()
        };

        _dbContext.PortalDocuments.Add(document);

        await _dbContext.SaveChangesAsync();

        var version = new PortalDocumentVersion
        {
            DocumentId = document.DocumentId,
            VersionNo = 1,
            OriginalFileName = file.FileName,
            FileType = Path.GetExtension(file.FileName)
                .TrimStart('.')
                .ToLowerInvariant(),
            SharePointItemId = sharePointItemId,
            FileSize = file.Length,
            UploadedBy = uploadedBy,
            UploadedAt = GetRiyadhNow(),
            IsCurrent = 1
        };

        _dbContext.PortalDocumentVersions.Add(version);

        await _dbContext.SaveChangesAsync();
    }

    public async Task CreateNewVersionAsync(
        int documentId,
        IFormFile file,
        string sharePointItemId,
        string uploadedBy)
    {
        var document = await _dbContext.PortalDocuments
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x =>
                x.DocumentId == documentId &&
                x.Status == 1);

        if (document == null)
        {
            throw new InvalidOperationException(
                "Document was not found.");
        }

        var currentVersions = document.Versions
            .Where(x => x.IsCurrent == 1)
            .ToList();

        foreach (var currentVersion in currentVersions)
        {
            currentVersion.IsCurrent = 0;
        }

        var nextVersionNumber = document.Versions.Any()
            ? document.Versions.Max(x => x.VersionNo) + 1
            : 1;

        var version = new PortalDocumentVersion
        {
            DocumentId = document.DocumentId,
            VersionNo = nextVersionNumber,
            OriginalFileName = file.FileName,
            FileType = Path.GetExtension(file.FileName)
                .TrimStart('.')
                .ToLowerInvariant(),
            SharePointItemId = sharePointItemId,
            FileSize = file.Length,
            UploadedBy = uploadedBy,
            UploadedAt = GetRiyadhNow(),
            IsCurrent = 1
        };

        document.LastUpdatedBy = uploadedBy;
        document.LastUpdatedAt = GetRiyadhNow();

        _dbContext.PortalDocumentVersions.Add(version);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PortalDocumentVersion>> GetVersionHistoryAsync(
        int documentId)
    {
        return await _dbContext.PortalDocumentVersions
            .AsNoTracking()
            .Where(x => x.DocumentId == documentId)
            .OrderByDescending(x => x.VersionNo)
            .ToListAsync();
    }

    public async Task DeactivateDocumentAsync(
        int documentId,
        string updatedBy)
    {
        var document = await _dbContext.PortalDocuments
            .FirstOrDefaultAsync(x =>
                x.DocumentId == documentId);

        if (document == null)
        {
            throw new InvalidOperationException(
                "Document was not found.");
        }

        document.Status = 0;
        document.LastUpdatedBy = updatedBy;
        document.LastUpdatedAt = GetRiyadhNow();

        await _dbContext.SaveChangesAsync();
    }

    private static string NormalizeFolderPath(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return "";
        }

        return folderPath
            .Replace("\\", "/")
            .Trim()
            .Trim('/');
    }

    private static DateTime GetRiyadhNow()
    {
        var saudiTimeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");

        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            saudiTimeZone);
    }
}