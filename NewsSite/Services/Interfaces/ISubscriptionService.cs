using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
        Task CreateOrRenewAsync(string userId);

        Task CancelSubscriptionAsync(string userId);
        Task<int> GetRemainingSubscriptionDaysAsync(string userId);
        Task<(bool Success, string Message, int RemainingDays)> SpendSubscriptionDayForGameAsync(string userId);
    }
}
