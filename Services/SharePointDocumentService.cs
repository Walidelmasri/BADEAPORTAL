using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using BADEAPORTAL.Models.Documents;
using Microsoft.Extensions.Options;

namespace BADEAPORTAL.Services;

public sealed class SharePointDocumentService : ISharePointDocumentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly SharePointOptions _sharePointOptions;

    public SharePointDocumentService(
        HttpClient httpClient,
        IConfiguration configuration,
        IOptions<SharePointOptions> sharePointOptions)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _sharePointOptions = sharePointOptions.Value;
    }

    public async Task<IReadOnlyList<SharePointDocumentVm>> ListDocumentsAsync(string? folderPath)
    {
        await SetGraphAuthorizationHeaderAsync();

        var siteId = await GetSiteIdAsync();
        var driveId = await GetDriveIdAsync(siteId);

        return await GetDocumentsAsync(driveId, folderPath);
    }

    public async Task<(Stream Content, string FileName, string ContentType)> DownloadDocumentAsync(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Document item id is required.", nameof(itemId));
        }

        await SetGraphAuthorizationHeaderAsync();

        var siteId = await GetSiteIdAsync();
        var driveId = await GetDriveIdAsync(siteId);

        var metadataUrl =
            $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{itemId}?$select=name,file";

        using var metadataResponse = await _httpClient.GetAsync(metadataUrl);

        if (!metadataResponse.IsSuccessStatusCode)
        {
            var error = await metadataResponse.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Unable to retrieve SharePoint document metadata. Status: {metadataResponse.StatusCode}. Details: {error}");
        }

        using var metadataStream = await metadataResponse.Content.ReadAsStreamAsync();
        using var metadataDocument = await JsonDocument.ParseAsync(metadataStream);

        var fileName = metadataDocument.RootElement.GetProperty("name").GetString()
            ?? "document";

        var contentType = "application/octet-stream";

        if (metadataDocument.RootElement.TryGetProperty("file", out var fileElement) &&
            fileElement.TryGetProperty("mimeType", out var mimeTypeElement))
        {
            contentType = mimeTypeElement.GetString() ?? contentType;
        }

        var downloadUrl =
            $"https://graph.microsoft.com/v1.0/drives/{driveId}/items/{itemId}/content";

        var downloadResponse = await _httpClient.GetAsync(downloadUrl);

        if (!downloadResponse.IsSuccessStatusCode)
        {
            var error = await downloadResponse.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Unable to download SharePoint document. Status: {downloadResponse.StatusCode}. Details: {error}");
        }

        var content = await downloadResponse.Content.ReadAsStreamAsync();

        return (content, fileName, contentType);
    }

    private async Task SetGraphAuthorizationHeaderAsync()
    {
        var tenantId = _configuration["AzureAd:TenantId"];
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];

        if (string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException(
                "AzureAd configuration is missing required SharePoint Graph credentials.");
        }

        var credential = new ClientSecretCredential(
            tenantId,
            clientId,
            clientSecret);

        var token = await credential.GetTokenAsync(
            new TokenRequestContext(
                new[] { "https://graph.microsoft.com/.default" }));

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);
    }

    private async Task<string> GetSiteIdAsync()
    {
        var url =
            $"https://graph.microsoft.com/v1.0/sites/{_sharePointOptions.SiteHost}:{_sharePointOptions.SitePath}";

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Unable to resolve SharePoint site. Status: {response.StatusCode}. Details: {error}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        return document.RootElement
            .GetProperty("id")
            .GetString()
            ?? throw new InvalidOperationException(
                "SharePoint site id was not returned by Graph.");
    }

    private async Task<string> GetDriveIdAsync(string siteId)
    {
        var url =
            $"https://graph.microsoft.com/v1.0/sites/{siteId}/drives";

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Unable to retrieve SharePoint document libraries. Status: {response.StatusCode}. Details: {error}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        foreach (var drive in document.RootElement
                     .GetProperty("value")
                     .EnumerateArray())
        {
            var driveName = drive
                .GetProperty("name")
                .GetString();

            if (string.Equals(
                    driveName,
                    _sharePointOptions.LibraryName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return drive
                    .GetProperty("id")
                    .GetString()
                    ?? throw new InvalidOperationException(
                        "SharePoint drive id was not returned by Graph.");
            }
        }

        throw new InvalidOperationException(
            $"SharePoint library '{_sharePointOptions.LibraryName}' was not found.");
    }

    private async Task<IReadOnlyList<SharePointDocumentVm>> GetDocumentsAsync(
        string driveId,
        string? folderPath)
    {
        var url = BuildChildrenUrl(driveId, folderPath);

        using var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();

            throw new InvalidOperationException(
                $"Unable to retrieve SharePoint documents. Status: {response.StatusCode}. Details: {error}");
        }

        using var stream = await response.Content.ReadAsStreamAsync();
        using var document = await JsonDocument.ParseAsync(stream);

        var items = new List<SharePointDocumentVm>();
        var cleanFolderPath = NormalizeFolderPath(folderPath);

        foreach (var item in document.RootElement
                     .GetProperty("value")
                     .EnumerateArray())
        {
            var name = item.GetProperty("name").GetString() ?? "";
            var isFolder = item.TryGetProperty("folder", out _);

            items.Add(new SharePointDocumentVm
            {
                ItemId = item.GetProperty("id").GetString() ?? "",
                Name = name,
                WebUrl = item.GetProperty("webUrl").GetString() ?? "",
                FolderPath = isFolder
                    ? BuildChildFolderPath(cleanFolderPath, name)
                    : cleanFolderPath,
                Size = item.TryGetProperty("size", out var size)
                    ? size.GetInt64()
                    : null,
                LastModifiedDateTime =
                    item.TryGetProperty("lastModifiedDateTime", out var modified)
                        ? modified.GetDateTimeOffset()
                        : null,
                IsFolder = isFolder
            });
        }

        return items
            .OrderByDescending(x => x.IsFolder)
            .ThenBy(x => x.Name)
            .ToList();
    }

    private static string BuildChildrenUrl(string driveId, string? folderPath)
    {
        var select =
            "$select=id,name,webUrl,size,lastModifiedDateTime,file,folder";

        var cleanFolderPath = NormalizeFolderPath(folderPath);

        if (string.IsNullOrWhiteSpace(cleanFolderPath))
        {
            return $"https://graph.microsoft.com/v1.0/drives/{driveId}/root/children?{select}";
        }

        var encodedPath = EncodeFolderPath(cleanFolderPath);

        return $"https://graph.microsoft.com/v1.0/drives/{driveId}/root:/{encodedPath}:/children?{select}";
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

    private static string BuildChildFolderPath(string currentFolderPath, string folderName)
    {
        if (string.IsNullOrWhiteSpace(currentFolderPath))
        {
            return folderName;
        }

        return $"{currentFolderPath}/{folderName}";
    }

    private static string EncodeFolderPath(string folderPath)
    {
        var segments = folderPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(WebUtility.UrlEncode);

        return string.Join("/", segments);
    }
}