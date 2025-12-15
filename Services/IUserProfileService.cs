using BADEAPORTAL.Models;

namespace BADEAPORTAL.Services
{
    public interface IUserProfileService
    {
        UserProfileDto GetCurrentUser();
    }
}
