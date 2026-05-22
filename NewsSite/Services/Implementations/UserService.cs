using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Mapping;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public async Task<UserAdminViewModel> GetUsersForAdminAsync()
        {
            var users = await userRepository.GetAllUsersAsync();
            var roles = await userRepository.GetAllRolesAsync();
            var activeSubscriberUserIds = await userRepository.GetActiveSubscriberUserIdsAsync() ?? [];

            var model = new UserAdminViewModel
            {
                AvailableRoles = roles.Select(r => new SelectListItem(r.Name, r.Name)).ToList()
            };

            foreach (var user in users)
            {
                var userRoles = await userRepository.GetUserRolesAsync(user);
                var hasActiveSubscription = activeSubscriberUserIds.Contains(user.Id);
                model.Users.Add(user.ToDisplayInfo(userRoles.FirstOrDefault(), hasActiveSubscription));
            }

            return model;
        }

        public async Task<bool> UpdateUserRoleAsync(string userId, string newRole)
            => await userRepository.UpdateUserRoleAsync(userId, newRole);

        public async Task<bool> SoftDeleteUserAsync(string userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = true;
            return await userRepository.UpdateUserDetailsAsync(user);
        }

        public async Task<bool> RestoreUserAsync(string userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.IsDeleted = false;
            return await userRepository.UpdateUserDetailsAsync(user);
        }

        public async Task<bool> AnonymizeUserAsync(string userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);
            if (user == null) return false;

            user.FirstName = "Anonymous";
            user.LastName = "User";
            user.Email = $"deleted_{user.Id.Substring(0, 8)}@anonymized.com";
            user.NormalizedEmail = user.Email.ToUpper();
            user.UserName = user.Email;
            user.NormalizedUserName = user.UserName.ToUpper();
            user.PhoneNumber = null;
            user.PasswordHash = null;
            user.IsDeleted = true;

            return await userRepository.UpdateUserDetailsAsync(user);
        }
    }
}