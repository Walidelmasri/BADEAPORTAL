using BADEAPORTAL.Data;
using BADEAPORTAL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using QuestPDF.Infrastructure;
using System.Globalization;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

var supportedCultures = new[]
{
    new CultureInfo("en"),
    new CultureInfo("ar")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// MVC
builder.Services.AddControllersWithViews();

// HttpContext + user profile
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();

// Announcements + PDF
builder.Services.AddScoped<IAnnouncementsService, AnnouncementsService>();
builder.Services.AddScoped<IMemoPdfService, QuestPdfMemoService>();
builder.Services.AddScoped<IHtmlContentNormalizer, QuillHtmlNormalizer>();
builder.Services.AddScoped<IPickerDirectory, OraclePickerDirectory>();

builder.Services.Configure<SharePointOptions>(
    builder.Configuration.GetSection("SharePoint"));

builder.Services.AddHttpClient();

builder.Services.AddScoped<ISharePointDocumentService, SharePointDocumentService>();
builder.Services.AddScoped<IPortalDocumentService, PortalDocumentService>();

// Oracle EF Core
builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Entra ID auth
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

// A successful OIDC callback consumes its temporary correlation cookie.
// If an already-authenticated user presses Back and the browser replays that
// old /signin-oidc callback, treat only that stale replay as harmless and send
// the user back to the portal root. Genuine login failures are left untouched.
builder.Services.PostConfigure<OpenIdConnectOptions>(
    OpenIdConnectDefaults.AuthenticationScheme,
    options =>
    {
        var existingOnRemoteFailure = options.Events.OnRemoteFailure;

        options.Events.OnRemoteFailure = async context =>
        {
            var isCorrelationFailure =
                context.Failure?.ToString().Contains(
                    "Correlation failed",
                    StringComparison.OrdinalIgnoreCase) == true;

            if (isCorrelationFailure)
            {
                var cookieResult = await context.HttpContext.AuthenticateAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                var alreadySignedIn =
                    cookieResult.Succeeded &&
                    cookieResult.Principal?.Identity?.IsAuthenticated == true;

                if (alreadySignedIn)
                {
                    context.HandleResponse();
                    context.Response.Redirect("/");
                    return;
                }
            }

            if (existingOnRemoteFailure is not null)
            {
                await existingOnRemoteFailure(context);
            }
        };
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// The portal is hosted at the domain root: https://portal.internal.badea.org/
// Do not set a /portalbadea PathBase here.
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
