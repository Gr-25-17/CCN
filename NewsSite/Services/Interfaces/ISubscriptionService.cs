using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task CreateOrRenewAsync(string userId);

        Task CancelSubscriptionAsync(string userId);
    }
}
