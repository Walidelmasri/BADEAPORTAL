BADEA Portal - final browser Back safeguard

REPLACE:
1. Program.cs
2. Controllers/AccountController.cs
3. wwwroot/js/site.js

DELETE if you added it from the previous attempt:
4. Views/Account/SignedIn.cshtml

Do NOT change:
- AzureAd CallbackPath (/signin-oidc)
- Entra redirect URI (https://portal.internal.badea.org/signin-oidc)
- Error page
- Home/Index.cshtml
- Tool card links

The tool cards already use target="_blank" and rel="noopener noreferrer".
The history guard only activates at / or /Home/Index.
