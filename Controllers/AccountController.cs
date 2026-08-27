using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BADEAPORTAL.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = "/")
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);

            if (User.Identity?.IsAuthenticated == true)
            {
                return LocalRedirect(safeReturnUrl);
            }

            ViewData["ReturnUrl"] = safeReturnUrl;
            return View();
        }

        // POST: /Account/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(string? returnUrl = "/")
        {
            var safeReturnUrl = GetSafeReturnUrl(returnUrl);
            var redirectUrl = Url.Action(
                nameof(SignedIn),
                "Account",
                new { returnUrl = safeReturnUrl });

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(
                properties,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // Called after Entra completes sign-in.
        // No transition/history page is needed. The portal homepage itself
        // installs the browser-history guard from wwwroot/js/site.js.
        [HttpGet]
        public IActionResult SignedIn(string? returnUrl = "/")
        {
            return LocalRedirect(GetSafeReturnUrl(returnUrl));
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var callbackUrl = Url.Action(
                nameof(LoggedOut),
                "Account",
                values: null,
                protocol: Request.Scheme);

            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // Called after logout
        [HttpGet]
        public IActionResult LoggedOut()
        {
            return RedirectToAction("Index", "Home");
        }

        private string GetSafeReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return "/";
        }
    }
}
