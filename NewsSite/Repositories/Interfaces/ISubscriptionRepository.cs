using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task<Subscription?> GetLatestByUserIdAsync(string userId);
        Task<int> GetDefaultSubscriptionTypeIdAsync();
        Task SaveAsync(Subscription subscription);

        Task<Subscription?> GetActiveSubscriptionAsync(string userId);
    }
}
