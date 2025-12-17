using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BADEAPORTAL.Models;
using BADEAPORTAL.Services;
using BADEAPORTAL.Models.Home;
using Microsoft.AspNetCore.Authorization;

namespace BADEAPORTAL.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAnnouncementsService _announcements;

    public HomeController(
        ILogger<HomeController> logger,
        IAnnouncementsService announcements)
    {
        _logger = logger;
        _announcements = announcements;
    }

    public async Task<IActionResult> Index()
    {
        var (items, _) = await _announcements.GetPagedAsync(page: 1, pageSize: 3);

        var vm = new HomeIndexVm
        {
            LatestAnnouncements = items.Select(a => new AnnouncementPreviewVm
            {
                Id = a.Id,
                Title = a.Title,
                Excerpt = BuildExcerpt(a.BodyHtml, 140),
                IsMemo = a.IsMemo,
                CreatedAtUtc = a.CreatedAtUtc,
                CreatedByName = a.CreatedByName
            }).ToList()
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    private static string BuildExcerpt(string html, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;

        var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
        text = System.Net.WebUtility.HtmlDecode(text).Trim();

        if (text.Length <= maxLength) return text;
        return text.Substring(0, maxLength) + "…";
    }
}
