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
            // If already signed in, just go back
            if (User.Identity?.IsAuthenticated == true)
            {
                return Redirect(returnUrl ?? "/");
            }

            // Show your premium landing page with a "Sign in with BADEA" button
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/SignIn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn(string? returnUrl = "/")
        {
            var redirectUrl = Url.Action(nameof(SignedIn), "Account", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        // called after Entra completes sign-in
        [HttpGet]
        public IActionResult SignedIn(string? returnUrl = "/")
        {
            if (string.IsNullOrEmpty(returnUrl)) returnUrl = "/";
            return Redirect(returnUrl);
        }

        // GET: /Account/Logout
        [HttpGet]
        public IActionResult Logout()
        {
            var callbackUrl = Url.Action(nameof(LoggedOut), "Account", values: null, protocol: Request.Scheme);

            return SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        // called after logout
        [HttpGet]
        public IActionResult LoggedOut()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
