using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Authorize(Policy = "PremiumContent")]
    public class GameController(ISubscriptionService subscriptionService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var remainingDays = await subscriptionService.GetRemainingSubscriptionDaysAsync(userId);
            return View(new GameViewModel { RemainingSubscriptionDays = remainingDays });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyBallsWithSubscriptionDay()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var result = await subscriptionService.SpendSubscriptionDayForGameAsync(userId);
            return Json(new
            {
                success = result.Success,
                message = result.Message,
                remainingDays = result.RemainingDays
            });
        }
    }
}
