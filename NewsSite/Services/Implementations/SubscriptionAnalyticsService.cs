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

            var totalUsers = await customerUsers.CountAsync();

            var regThisMonth = 26;
            var regLastMonth = 15;

            var latestUnsubscribeStatusByUser = await _context.UnsubscribeLogs
                .Where(u => !internalUserIds.Contains(u.UserId))
                .GroupBy(u => u.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    IsDeactivated = !g.OrderByDescending(x => x.UnsubscribedAt)
                        .Select(x => x.WasReactivated)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var deactivatedUserIds = latestUnsubscribeStatusByUser
                .Where(x => x.IsDeactivated)
                .Select(x => x.UserId)
                .ToHashSet();

            var returningSubscribers = await (
                from unsubscribe in _context.UnsubscribeLogs
                where !internalUserIds.Contains(unsubscribe.UserId)
                join subscription in _context.Subscriptions on unsubscribe.UserId equals subscription.UserId
                where subscription.StartDate > unsubscribe.UnsubscribedAt
                select unsubscribe.UserId
            )
            .Distinct()
            .CountAsync();

            var activeSubs = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId) && !deactivatedUserIds.Contains(s.UserId))
                .CountAsync(s => s.StartDate <= now && s.EndDate > now);

            var inactiveSubs = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s => s.EndDate <= now || deactivatedUserIds.Contains(s.UserId));

            var newThisMonth = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s => s.StartDate >= startOfThisMonth);

            var newLastMonth = await _context.Subscriptions
                .Where(s => !internalUserIds.Contains(s.UserId))
                .CountAsync(s =>
                    s.StartDate >= startOfLastMonth &&
                    s.StartDate < startOfThisMonth);

            var churnThisMonth = await _context.UnsubscribeLogs
                .Where(u => !internalUserIds.Contains(u.UserId))
                .CountAsync(u => u.UnsubscribedAt >= startOfThisMonth);

            var churnLastMonth = await _context.UnsubscribeLogs
                .Where(u => !internalUserIds.Contains(u.UserId))
                .CountAsync(u =>
                    u.UnsubscribedAt >= startOfLastMonth &&
                    u.UnsubscribedAt < startOfThisMonth);


            var subscriberPercentage = totalUsers == 0
                ? 0
                : Math.Round((double)activeSubs / totalUsers * 100, 1);

            var churnRate = activeSubs == 0
                ? 0
                : Math.Round((double)churnThisMonth / activeSubs * 100, 1);

            var estimatedRevenue = activeSubs * 99;

            var writerStats = await BuildWriterStatsAsync(startOfThisMonth, totalUsers);

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
                EstimatedMonthlyRevenue = estimatedRevenue,

                TrendLabels = writerStats.TrendLabels,
                TrendSubscriberCounts = writerStats.TrendSubscriberCounts,
                TrendUserCounts = writerStats.TrendUserCounts,
                WriterPerformances = writerStats.WriterPerformances,
                WriterMonthlyTrends = writerStats.WriterMonthlyTrends
            };
        }

        private async Task<(List<WriterPerformanceDto> WriterPerformances, List<WriterMonthlyTrendDto> WriterMonthlyTrends, List<string> TrendLabels, List<int> TrendSubscriberCounts, List<int> TrendUserCounts)> BuildWriterStatsAsync(DateTime startOfThisMonth, int totalUsers)
        {
            var authors = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where role.Name == "Writer"
                select new
                {
                    user.Id,
                    user.FirstName,
                    user.LastName,
                    user.Email
                }
            ).Distinct().ToListAsync();

            var writerIds = authors.Select(a => a.Id).ToList();

            var writerArticles = await _context.Articles
                .Include(a => a.Likes)
                .Where(a => !a.IsDeleted && a.AuthorId != null && writerIds.Contains(a.AuthorId))
                .ToListAsync();

            var writerPerformances = authors
                .Select(author =>
                {
                    var articles = writerArticles.Where(a => a.AuthorId == author.Id).ToList();
                    var totalArticles = articles.Count;
                    var articlesThisMonth = articles.Count(a => a.CreatedAt >= startOfThisMonth);
                    var totalLikes = articles.Sum(a => a.Likes.Count);
                    var totalViews = articles.Sum(a => a.ViewsCount);
                    var totalEngagement = totalLikes + totalViews;
                    var avgEngagementPerArticle = totalArticles == 0 ? 0 : Math.Round((double)totalEngagement / totalArticles, 1);
                    var revenueEstimate = Math.Round((decimal)totalViews * 0.05m, 2);

                    const double articleWeight = 2.0;
                    const double likesWeight = 1.5;
                    const double viewsWeight = 0.02;
                    var impactScore = Math.Round((articlesThisMonth * articleWeight) + (avgEngagementPerArticle * likesWeight) + (totalViews * viewsWeight), 1);

                    var displayName = $"{author.FirstName} {author.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(displayName))
                    {
                        displayName = author.Email ?? "Unknown writer";
                    }

                    return new WriterPerformanceDto
                    {
                        AuthorId = author.Id,
                        AuthorName = displayName,
                        ArticlesThisMonth = articlesThisMonth,
                        TotalArticles = totalArticles,
                        TotalLikes = totalLikes,
                        TotalViews = totalViews,
                        AvgEngagementPerArticle = avgEngagementPerArticle,
                        RevenueEstimate = revenueEstimate,
                        ImpactScore = impactScore
                    };
                })
                .OrderByDescending(w => w.ImpactScore)
                .ToList();

            var trendStart = startOfThisMonth.AddMonths(-5);
            var writerMonthlyTrends = writerArticles
                .Where(a => a.CreatedAt >= trendStart)
                .GroupBy(a => new { a.AuthorId, Month = new DateTime(a.CreatedAt.Year, a.CreatedAt.Month, 1) })
                .Select(g =>
                {
                    var author = authors.FirstOrDefault(a => a.Id == g.Key.AuthorId);
                    var displayName = author == null
                        ? "Unknown writer"
                        : $"{author.FirstName} {author.LastName}".Trim();

                    if (string.IsNullOrWhiteSpace(displayName) && author?.Email != null)
                    {
                        displayName = author.Email;
                    }

                    return new WriterMonthlyTrendDto
                    {
                        AuthorId = g.Key.AuthorId ?? string.Empty,
                        AuthorName = displayName,
                        MonthLabel = g.Key.Month.ToString("yyyy-MM"),
                        Articles = g.Count(),
                        Engagement = g.Sum(x => x.Likes.Count + x.ViewsCount)
                    };
                })
                 .OrderByDescending(t => t.Engagement)
                .ThenBy(t => t.MonthLabel)
                .ToList();

            var trendStartMonth = new DateTime(startOfThisMonth.Year, startOfThisMonth.Month, 1).AddMonths(-5);
            var trendLabels = Enumerable.Range(0, 6)
                .Select(offset => trendStartMonth.AddMonths(offset))
                .Select(month => month.ToString("yyyy-MM"))
                .ToList();

            var trendSubscriberCounts = new List<int>();
            var trendUserCounts = new List<int>();

            foreach (var label in trendLabels)
            {
                var monthStart = DateTime.ParseExact(label + "-01", "yyyy-MM-dd", null);
                var monthEnd = monthStart.AddMonths(1);

                var activeSubscriberCount = await _context.Subscriptions
                    .Where(s => s.StartDate < monthEnd && s.EndDate >= monthStart)
                    .Select(s => s.UserId)
                    .Distinct()
                    .CountAsync();

                var userCount = totalUsers;

                trendSubscriberCounts.Add(activeSubscriberCount);
                trendUserCounts.Add(userCount);
            }

            return (writerPerformances, writerMonthlyTrends, trendLabels, trendSubscriberCounts, trendUserCounts);
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
