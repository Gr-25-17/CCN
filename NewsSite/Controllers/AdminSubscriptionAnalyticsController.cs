using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionAnalyticsController(ISubscriptionAnalyticsService subscriptionAnalyticsService) : Controller
    {
        public IActionResult Index() => View();

        [HttpGet("api/admin/subscription-stats")]
        public async Task<IActionResult> GetSubscriptionStats()
        {
            var stats = await subscriptionAnalyticsService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
