using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SubscriptionAnalyticsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}