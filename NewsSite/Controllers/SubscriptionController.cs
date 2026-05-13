using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Authorize]
    public class SubscriptionController(ISubscriptionService subscriptionService) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(PaymentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            await subscriptionService.CreateOrRenewAsync(userId);

            return RedirectToAction(nameof(Success));
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
