using BADEAPORTAL.Data;
using BADEAPORTAL.Services;
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

// Recover cleanly from a stale/replayed OIDC callback.
// This commonly happens when a browser revisits /signin-oidc with the Back button
// after the one-time correlation cookie has already been consumed.
//
// We do NOT treat other authentication failures as harmless: they continue through
// Microsoft.Identity.Web's normal failure handling and can reach the normal error page.
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
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("OidcCorrelationRecovery");

                logger.LogInformation(
                    "Recovered from stale or invalid OIDC correlation callback at {Path}. Redirecting to a fresh login state.",
                    context.Request.Path);

                context.HandleResponse();

                // Account/Login is AllowAnonymous.
                // If the normal application cookie is still valid, AccountController.Login
                // immediately redirects the user back to '/'. If it is not valid, the user
                // receives the normal sign-in page and can start a fresh OIDC transaction.
                context.Response.Redirect("/Account/Login?returnUrl=%2F");
                return;
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
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
