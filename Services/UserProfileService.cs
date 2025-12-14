using System.Security.Claims;
using BADEAPORTAL.Models;

namespace BADEAPORTAL.Services
{
    public interface IUserProfileService
    {
        UserProfileDto GetCurrentUser();
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserProfileService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public UserProfileDto GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                return new UserProfileDto();
            }

            string? name =
                user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("name")?.Value;

            string? email =
                user.FindFirst(ClaimTypes.Email)?.Value
                ?? user.FindFirst("preferred_username")?.Value;

            return new UserProfileDto
            {
                DisplayName = name,
                Email = email
            };
        }
    }
}
