using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations
{
    public class UserRepository(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager) : IUserRepository
    {
        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync() => await userManager.Users.ToListAsync();
        public async Task<IEnumerable<IdentityRole>> GetAllRolesAsync() => await roleManager.Roles.ToListAsync();
        public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user) => await userManager.GetRolesAsync(user);

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var currentRoles = await userManager.GetRolesAsync(user);
            await userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await userManager.AddToRoleAsync(user, newRole);
            return result.Succeeded;
        }
    }
}