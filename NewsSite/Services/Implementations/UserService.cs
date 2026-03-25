using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Implementations
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<UserAdminViewModel> GetUsersForAdminAsync()
        {
            var users = await userRepository.GetAllUsersAsync();
            var roles = await userRepository.GetAllRolesAsync();
            var model = new UserAdminViewModel { AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList() };

            foreach (var user in users)
            {
                var userRoles = await userRepository.GetUserRolesAsync(user);
                model.Users.Add(new UserDisplayInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    CurrentRole = userRoles.FirstOrDefault() ?? "Ingen roll"
                });
            }
            return model;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole) => await userRepository.UpdateUserRoleAsync(userId, newRole);
    }
}