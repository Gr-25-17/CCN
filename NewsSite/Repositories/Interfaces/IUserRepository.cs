using Microsoft.AspNetCore.Identity;
using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<IEnumerable<IdentityRole>> GetAllRolesAsync();
        Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
    }
}