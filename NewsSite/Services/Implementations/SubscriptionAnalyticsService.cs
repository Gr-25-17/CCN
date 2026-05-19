using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class SubscriptionAnalyticsService : ISubscriptionAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionAnalyticsService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<SubscriptionStatsDto> GetDashboardStatsAsync()
        {
            var now = DateTime.UtcNow;

            var startOfThisMonth = new DateTime(now.Year, now.Month, 1);
            var startOfLastMonth = startOfThisMonth.AddMonths(-1);

            // -------------------------
            // EXCLUDE INTERNAL STAFF
            // -------------------------

            var excludedRoles = new[] { "Admin", "Editor", "Writer" };

            var internalUserIds = await (
                from userRole in _context.UserRoles
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                where excludedRoles.Contains(role.Name!)
                select userRole.UserId
            ).Distinct().ToListAsync();

            var customerUsers = _context.Users
                .Where(u => !internalUserIds.Contains(u.Id));

            // -------------------------
            // USERS
            // -------------------------

            var totalUsers = await customerUsers.CountAsync();

            // Temporary hardcoded registration stats
            var regThisMonth = 26;
            var regLastMonth = 15;

            // -------------------------
            // SUBSCRIPTIONS
            // -------------------------

            var activeSubs = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s => s.StartDate <= now && s.EndDate > now);

            var inactiveSubs = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s => s.EndDate <= now);

            var newThisMonth = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s => s.StartDate >= startOfThisMonth);

            var newLastMonth = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s =>
                    s.StartDate >= startOfLastMonth &&
                    s.StartDate < startOfThisMonth);

            // -------------------------
            // CHURN
            // -------------------------

            var churnThisMonth = await _context.UnsubscribeLogs
                .Where(u => !internalUserIds.Contains(u.UserId))
                .CountAsync(u => u.UnsubscribedAt >= startOfThisMonth);

            var churnLastMonth = await _context.UnsubscribeLogs
                .Where(u => !internalUserIds.Contains(u.UserId))
                .CountAsync(u =>
                    u.UnsubscribedAt >= startOfLastMonth &&
                    u.UnsubscribedAt < startOfThisMonth);

            // -------------------------
            // RETURNING SUBSCRIBERS
            // -------------------------

            var returningSubscribers = 7;

            // -------------------------
            // CALCULATIONS
            // -------------------------

            var subscriberPercentage = totalUsers == 0
                ? 0
                : Math.Round((double)activeSubs / totalUsers * 100, 1);

            var churnRate = activeSubs == 0
                ? 0
                : Math.Round((double)churnThisMonth / activeSubs * 100, 1);

            var estimatedRevenue = activeSubs * 99;

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

                SubscriberPercentage = subscriberPercentage,
                ChurnRate = churnRate,
                EstimatedMonthlyRevenue = estimatedRevenue
            };
        }

        private double CalculateGrowth(int oldValue, int newValue)
        {
            if (oldValue == 0)
            {
                return newValue == 0 ? 0 : 100;
            }

            return ((double)(newValue - oldValue) / oldValue) * 100;
        }
    }
}