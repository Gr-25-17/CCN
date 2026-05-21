using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NewsSite.Controllers
{
    [Authorize(Policy = "PremiumContent")]
    public class GameController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
