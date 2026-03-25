using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using NewsSite.Models.Entities;

namespace NewsSite.Services.Implementations
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<UserAdminViewModel> GetUsersForAdminAsync()
        {
            var users = await userRepository.GetAllUsersAsync();
            var roles = await userRepository.GetAllRolesAsync();

            var model = new UserAdminViewModel
            {
                AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList()
            };

            foreach (var user in users)
            {
                var userRoles = await userRepository.GetUserRolesAsync(user);
                model.Users.Add(new UserDisplayInfo
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = $"{user.FirstName} {user.LastName}",
                    DateOfBirth = user.DateOfBirth,
                    CurrentRole = userRoles.FirstOrDefault() ?? "Ingen roll",
                    IsDeleted = user.IsDeleted
                });
            }

            return model;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
        {
            return await userRepository.UpdateUserRoleAsync(userId, newRole);
        }

        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            return await userRepository.UpdateUserDetailsAsync(user);
        }
    }
}