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

        
            var totalUsers = await _context.Users.CountAsync();

            //var regThisMonth = await _context.Users
            //    .CountAsync(u => u.CreatedAt >= startOfThisMonth);

            //var regLastMonth = await _context.Users
            //    .CountAsync(u => u.CreatedAt >= startOfLastMonth &&
            //                     u.CreatedAt < startOfThisMonth);
            var regThisMonth = 26;
            var regLastMonth = 15;

            var activeSubs = await _context.Subscriptions
                .CountAsync(s => s.StartDate <= now && s.EndDate > now);

            var inactiveSubs = await _context.Subscriptions
                .CountAsync(s => s.EndDate <= now);

     
            var newThisMonth = await _context.Subscriptions
                .CountAsync(s => s.StartDate >= startOfThisMonth);

            var newLastMonth = await _context.Subscriptions
                .CountAsync(s => s.StartDate >= startOfLastMonth &&
                                 s.StartDate < startOfThisMonth);

        
            var churnThisMonth = await _context.UnsubscribeLogs
                .CountAsync(u => u.UnsubscribedAt >= startOfThisMonth);

            var churnLastMonth = await _context.UnsubscribeLogs
                .CountAsync(u => u.UnsubscribedAt >= startOfLastMonth &&
                                 u.UnsubscribedAt < startOfThisMonth);

            //var returningSubscribers = await _context.UnsubscribeLogs
            //    .CountAsync(u =>
            //        u.WasReactivated &&
            //        u.ReactivatedAt != null &&
            //        EF.Functions.DateDiffDay(u.UnsubscribedAt, u.ReactivatedAt.Value) >= 1
            //    );
            var returningSubscribers = 7;


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

              
            };
        }

        private double CalculateGrowth(int oldValue, int newValue)
        {
            if (oldValue == 0) return newValue == 0 ? 0 : 100;
            return ((double)(newValue - oldValue) / oldValue) * 100;
        }
    }
}