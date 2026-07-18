using System.Diagnostics;
using BADEAPORTAL.Data;
using BADEAPORTAL.Models;
using BADEAPORTAL.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly PortalDbContext _db;

    public HomeController(
        ILogger<HomeController> logger,
        PortalDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var heroSlides = await _db.PortalHeroSlides
                .AsNoTracking()
                .Where(x => x.IsActive == 1)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SlideId)
                .Select(x => new PortalHeroSlideVm
                {
                    SlideId = x.SlideId,
                    ImagePath = x.ImagePath,
                    AltTextEn = x.AltTextEn,
                    AltTextAr = x.AltTextAr
                })
                .ToListAsync();

            var cards = await _db.PortalSystemCards
                .AsNoTracking()
                .Where(x => x.IsActive == 1)
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
                HeroSlides = heroSlides,
                SystemCards = cards,
                LatestAnnouncements = new List<AnnouncementPreviewVm>()
            };

            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to load portal homepage for {User}. Trace ID: {TraceId}",
                User.Identity?.Name,
                HttpContext.TraceIdentifier);

            throw;
        }
    }

    [HttpGet]
    public IActionResult SetLanguage(string culture, string returnUrl = "/")
    {
        if (culture != "ar" && culture != "en")
        {
            culture = "en";
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(
                new RequestCulture(culture)),
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

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId =
                Activity.Current?.Id ??
                HttpContext.TraceIdentifier
        });
    }
}