using Microsoft.AspNetCore.Mvc;

namespace NewsSite.ViewComponents
{
    public class LoginFormVC : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
