using BADEAPORTAL.Data;
using BADEAPORTAL.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
// ...

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

// Oracle EF Core
builder.Services.AddDbContext<PortalDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// Entra ID auth
builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UsePathBase("/portalbadea");
// Static files MUST be before routing
app.UseStaticFiles();
app.UseRequestLocalization();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
