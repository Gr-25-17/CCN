using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class SubscriptionAnalyticsService : ISubscriptionAnalyticsService
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionAnalyticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SubscriptionStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;

            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            // -------------------------
            // USERS
            // -------------------------
            var totalUsers = await _context.Users.CountAsync();

            //var regThisMonth = await _context.Users
            //    .CountAsync(u => u.CreatedAt >= startOfThisMonth);

            //var regLastMonth = await _context.Users
            //    .CountAsync(u => u.CreatedAt >= startOfLastMonth &&
            //                     u.CreatedAt < startOfThisMonth);
            var regThisMonth = 0;
            var regLastMonth = 0;

            // -------------------------
            // SUBSCRIPTIONS (ACTIVE STATE)
            // -------------------------
            var activeSubs = await _context.Subscriptions
                .CountAsync(s => s.StartDate <= now && s.EndDate > now);

            var inactiveSubs = await _context.Subscriptions
                .CountAsync(s => s.EndDate <= now);

            // -------------------------
            // NEW SUBSCRIPTIONS
            // -------------------------
            var newThisMonth = await _context.Subscriptions
                .CountAsync(s => s.StartDate >= startOfThisMonth);

            var newLastMonth = await _context.Subscriptions
                .CountAsync(s => s.StartDate >= startOfLastMonth &&
                                 s.StartDate < startOfThisMonth);

            // -------------------------
            // CHURN (from UnsubscribeLog)
            // -------------------------
            var churnThisMonth = await _context.UnsubscribeLogs
                .CountAsync(u => u.UnsubscribedAt >= startOfThisMonth);

            var churnLastMonth = await _context.UnsubscribeLogs
                .CountAsync(u => u.UnsubscribedAt >= startOfLastMonth &&
                                 u.UnsubscribedAt < startOfThisMonth);

            // -------------------------
            // RETURNING SUBSCRIBERS
            // Placeholder until we persist enough data to verify
            // prior subscription status before reactivation.
            // -------------------------
            var returningSubscribers = 0;

            // -------------------------
            // MOST COMMON UNSUBSCRIBE REASONS
            // (optional but powerful)
            // -------------------------
            var topReasons = await _context.UnsubscribeLogs
                .Where(u => !string.IsNullOrEmpty(u.Reason))
                .GroupBy(u => u.Reason)
                .Select(g => new
                {
                    Reason = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            // -------------------------
            // RESULT
            // -------------------------
            return new SubscriptionStatsDto
            {
                TotalRegisteredUsers = totalUsers,

                ActiveSubscribers = activeSubs,
                InactiveSubscribers = inactiveSubs,

                NewSubscribersThisMonth = newThisMonth,
                NewSubscribersLastMonth = newLastMonth,

                RegistrationsThisMonth = regThisMonth,
                RegistrationsLastMonth = regLastMonth,

                ReturningSubscribers = returningSubscribers,

                GrowthRateSubscribers = CalculateGrowth(newLastMonth, newThisMonth),
                GrowthRateRegistrations = CalculateGrowth(regLastMonth, regThisMonth),

                // OPTIONAL: you can extend DTO for this
                // TopUnsubscribeReasons = topReasons
            };
        }

        private double CalculateGrowth(int oldValue, int newValue)
        {
            if (oldValue == 0) return newValue == 0 ? 0 : 100;
            return ((double)(newValue - oldValue) / oldValue) * 100;
        }
    }
}