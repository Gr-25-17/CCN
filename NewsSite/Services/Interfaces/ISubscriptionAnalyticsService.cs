using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface ISubscriptionAnalyticsService
    {
        Task<SubscriptionStatsDto> GetDashboardStatsAsync();
    }
}
