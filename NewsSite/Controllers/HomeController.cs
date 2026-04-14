using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
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
        private readonly INewsletterService _newsletterService;
        private readonly ApplicationDbContext? _context;

        public HomeController(
            IArticleService articleService,
            ICategoryService categoryService,
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager,
            INewsletterService newsletterService,
            ApplicationDbContext context)  
        {
            _articleService = articleService;
            _categoryService = categoryService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _newsletterService = newsletterService;
            _context = context; 
        }

        public async Task<IActionResult> Index()
        {
            var hasSubscription = false;
            List<int> preferredCategoryIds = null;

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);

             
                if (!string.IsNullOrEmpty(userId))
                {
                    hasSubscription = await _subscriptionService.HasActiveSubscriptionAsync(userId);

                    var preferences = await _context.NewsletterPreferences
                        .FirstOrDefaultAsync(p => p.UserId == userId);

                    if (preferences != null && !string.IsNullOrEmpty(preferences.SelectedCategoryIds))
                    {
                        preferredCategoryIds = preferences.SelectedCategoryIds
                            .Split(',')
                            .Select(int.Parse)
                            .ToList();
                    }
                }
            }

            IEnumerable<Article> latestArticles;
            IEnumerable<Article> mostPopularArticles;
            IEnumerable<Article> editorChoiceArticles;

            if (preferredCategoryIds != null && preferredCategoryIds.Any())
            {
                latestArticles = await _articleService.GetLatestByCategoryIdsAsync(preferredCategoryIds, 6);
                mostPopularArticles = await _articleService.GetMostPopularByCategoryIdsAsync(preferredCategoryIds, 6);
                editorChoiceArticles = await _articleService.GetEditorChoiceByCategoryIdsAsync(preferredCategoryIds, 3);
            }
            else
            {
                latestArticles = await _articleService.GetLatestAsync(6);
                mostPopularArticles = await _articleService.GetMostPopularAsync(6);
                editorChoiceArticles = await _articleService.GetEditorChoiceAsync(3);
            }

            var viewModel = new HomeViewModel
            {
                LatestArticles = latestArticles,
                MostPopularArticles = mostPopularArticles,
                EditorChoiceArticles = editorChoiceArticles,
                Categories = await _categoryService.GetAllAsync()
            };

            ViewBag.HasSubscription = hasSubscription;

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