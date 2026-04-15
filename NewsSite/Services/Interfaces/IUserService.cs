using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserAdminViewModel> GetUsersForAdminAsync();
        Task<bool> UpdateUserRoleAsync(string userId, string newRole);
        Task<bool> SoftDeleteUserAsync(string userId);
        Task<bool> RestoreUserAsync(string userId);
        Task<bool> AnonymizeUserAsync(string userId);
    }
}