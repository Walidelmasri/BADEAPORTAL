using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BADEAPORTAL.Models;
using BADEAPORTAL.Services;
using BADEAPORTAL.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using BADEAPORTAL.Data;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAnnouncementsService _announcements;
    private readonly PortalDbContext _db;

    public HomeController(
        ILogger<HomeController> logger,
        IAnnouncementsService announcements,
        PortalDbContext db)
    {
        _logger = logger;
        _announcements = announcements;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var (items, _) = await _announcements.GetPagedAsync(page: 1, pageSize: 3);

        // var cards = await _db.PortalSystemCards
        //     .AsNoTracking()
        //     .Where(x => x.IsActive)
        //     .OrderBy(x => x.CardId)
        //     .Select(x => new PortalSystemCardVm
        //     {
        //         CardId = x.CardId,
        //         SysNameEn = x.SysNameEn,
        //         SysNameAr = x.SysNameAr,
        //         DescriptionEn = x.DescriptionEn,
        //         DescriptionAr = x.DescriptionAr,
        //         CategoryEn = x.CategoryEn,
        //         CategoryAr = x.CategoryAr,
        //         AppUrl = x.AppUrl,
        //         LogoPath = x.LogoPath
        //     })
        //     .ToListAsync();
        var cards = await _db.PortalSystemCards
            .AsNoTracking()
            // .Where(x => x.IsActive == 1)
            .OrderBy(x => x.CardId)
            .Select(x => new PortalSystemCardVm
            {
                CardId = x.CardId,
                SysNameEn = x.SysNameEn,
                SysNameAr = x.SysNameAr,
                DescriptionEn = x.DescriptionEn,
                DescriptionAr = x.DescriptionAr,
                CategoryEn = x.CategoryEn,
                CategoryAr = x.CategoryAr,
                AppUrl = x.AppUrl,
                LogoPath = x.LogoPath
            })
            .ToListAsync();
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
            }).ToList(),

            SystemCards = cards
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        if (culture != "ar" && culture != "en")
            culture = "en";

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });

        return LocalRedirect(returnUrl);
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