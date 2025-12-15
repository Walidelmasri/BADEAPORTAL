using System.Security.Claims;
using BADEAPORTAL.Models;

namespace BADEAPORTAL.Services
{
    public sealed class UserProfileService : IUserProfileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserProfileDto GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user is null || user.Identity?.IsAuthenticated != true)
            {
                return new UserProfileDto();
            }

            // Prefer formal full name (GivenName + Surname)
            var givenName =
                user.FindFirstValue(ClaimTypes.GivenName) ??
                user.FindFirstValue("given_name");

            var surname =
                user.FindFirstValue(ClaimTypes.Surname) ??
                user.FindFirstValue("family_name");

            var fullName = BuildFullName(givenName, surname);

            // Fallback to Display Name
            var displayName =
                user.FindFirstValue(ClaimTypes.Name) ??
                user.FindFirstValue("name");

            // Helpful identifier fallback (often present)
            var emailOrUpn =
                user.FindFirstValue(ClaimTypes.Email) ??
                user.FindFirstValue("preferred_username") ??
                user.FindFirstValue("upn");

            return new UserProfileDto
            {
                FullName = fullName,
                DisplayName = Normalize(displayName),
                EmailOrUpn = Normalize(emailOrUpn)
            };
        }

        private static string? BuildFullName(string? givenName, string? surname)
        {
            givenName = Normalize(givenName);
            surname = Normalize(surname);

            if (!string.IsNullOrWhiteSpace(givenName) && !string.IsNullOrWhiteSpace(surname))
                return $"{givenName} {surname}";

            // If only one part exists, still return it (formal enough)
            if (!string.IsNullOrWhiteSpace(givenName))
                return givenName;

            if (!string.IsNullOrWhiteSpace(surname))
                return surname;

            return null;
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return value.Trim();
        }
    }
}
