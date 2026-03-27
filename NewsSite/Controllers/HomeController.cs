using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Diagnostics;

namespace NewsSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICategoryService _categoryService;

        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(IArticleService articleService, ICategoryService categoryService,
            ISubscriptionService subscriptionService, UserManager<ApplicationUser> userManager)
        {
            _articleService = articleService;
            _categoryService = categoryService;

            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var hasSubscription = false;
            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                hasSubscription = await _subscriptionService.HasActiveSubscriptionAsync(userId);
            }
            var viewModel = new HomeViewModel
            {
                LatestArticles = await _articleService.GetLatestAsync(6),
                MostPopularArticles = await _articleService.GetMostPopularAsync(6),
                EditorChoiceArticles = await _articleService.GetEditorChoiceAsync(3),
                Categories = await _categoryService.GetAllAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}