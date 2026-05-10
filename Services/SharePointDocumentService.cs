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

    public async Task<IReadOnlyList<SharePointDocumentVm>> ListDocumentsAsync()
    {
        await SetGraphAuthorizationHeaderAsync();

        var siteId = await GetSiteIdAsync();
        var driveId = await GetDriveIdAsync(siteId);

        return await GetRootDocumentsAsync(driveId);
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

    private async Task<IReadOnlyList<SharePointDocumentVm>> GetRootDocumentsAsync(string driveId)
    {
        var url =
            $"https://graph.microsoft.com/v1.0/drives/{driveId}/root/children?$select=id,name,webUrl,size,lastModifiedDateTime,file,folder";

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

        foreach (var item in document.RootElement
                     .GetProperty("value")
                     .EnumerateArray())
        {
            items.Add(new SharePointDocumentVm
            {
                ItemId = item.GetProperty("id").GetString() ?? "",
                Name = item.GetProperty("name").GetString() ?? "",
                WebUrl = item.GetProperty("webUrl").GetString() ?? "",
                Size = item.TryGetProperty("size", out var size)
                    ? size.GetInt64()
                    : null,
                LastModifiedDateTime =
                    item.TryGetProperty("lastModifiedDateTime", out var modified)
                        ? modified.GetDateTimeOffset()
                        : null,
                IsFolder = item.TryGetProperty("folder", out _)
            });
        }

        return items
            .OrderBy(x => x.Name)
            .ToList();
    }
}