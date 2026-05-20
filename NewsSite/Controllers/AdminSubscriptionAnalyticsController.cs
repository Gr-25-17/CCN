using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionAnalyticsController : Controller
    {
        private readonly ISubscriptionAnalyticsService _subscriptionAnalyticsService;

        public SubscriptionAnalyticsController(ISubscriptionAnalyticsService subscriptionAnalyticsService)
        {
            _subscriptionAnalyticsService = subscriptionAnalyticsService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("api/admin/subscription-stats")]
        public async Task<IActionResult> GetSubscriptionStats()
        {
            var stats = await _subscriptionAnalyticsService.GetDashboardStatsAsync();
            return Ok(stats);
        }
    }
}
