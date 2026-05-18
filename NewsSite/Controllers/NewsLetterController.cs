using Microsoft.AspNetCore.Mvc;

namespace NewsSite.Controllers
{
    public class NewsLetterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
