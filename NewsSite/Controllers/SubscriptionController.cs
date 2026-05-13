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
        public IActionResult Index(string? returnUrl = null)
        {
            return View(new PaymentViewModel
            {
                CardName = string.Empty,
                CardNumber = string.Empty,
                ExpirationDate = string.Empty,
                CVV = string.Empty,
                ReturnUrl = returnUrl
            });
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

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return LocalRedirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
