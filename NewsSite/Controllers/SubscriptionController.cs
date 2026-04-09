using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace NewsSite.Controllers
{
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

            // Om användaren inte är inloggad kan vi inte skapa en prenumeration
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var success = await subscriptionService.HasActiveSubscriptionAsync(userId);

            // Här simulerar vi att vi skapar prenumerationen om den inte finns
            // (Du kan utöka ISubscriptionService med en Create-metod om det behövs)
            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}