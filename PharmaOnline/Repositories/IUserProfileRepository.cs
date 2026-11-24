using PharmaOnline.Models;

namespace PharmaOnline.Repositories;

public interface IUserProfileRepository
{
    Task<ApplicationUser?> GetProfileAsync(string userId);
    Task UpdateProfileAsync(ApplicationUser user);
}
