using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class UserExtensions
    {
        public static UserDisplayInfo ToDisplayInfo(this ApplicationUser user, string? currentRole, bool hasActiveSubscription)
            => new()
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = $"{user.FirstName} {user.LastName}",
                DateOfBirth = user.DateOfBirth,
                CurrentRole = currentRole ?? "Ingen roll",
                IsDeleted = user.IsDeleted,
                HasActiveSubscription = hasActiveSubscription
            };
    }
}
