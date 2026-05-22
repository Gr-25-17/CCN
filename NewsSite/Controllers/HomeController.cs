using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NewsSite.Mapping;
using NewsSite.Models;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Implementations;
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
        private readonly IWeatherService _weatherService;
        private readonly ISubscriptionAnalyticsService _subscriptionAnalyticsService;


        public HomeController(
            IArticleService articleService,
            ICategoryService categoryService,
            ISubscriptionService subscriptionService,
            UserManager<ApplicationUser> userManager,
            INewsletterService newsletterService,
            IWeatherService weatherService,
            ISubscriptionAnalyticsService subscriptionAnalyticsService)
        {
            _articleService = articleService;
            _categoryService = categoryService;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
            _newsletterService = newsletterService;
            _weatherService = weatherService;
            _subscriptionAnalyticsService = subscriptionAnalyticsService;
        }

        public async Task<IActionResult> Index()
        {
            var searchTerm = HttpContext.Request.Query["searchTerm"].ToString();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                return View(await BuildSearchViewModelAsync(searchTerm));
            }

            return View(await BuildDefaultViewModelAsync());
        }


        private async Task<(IEnumerable<ArticleSummaryViewModel> MostPopularArticles, IEnumerable<ArticleSummaryViewModel> EditorChoiceArticles)> GetSidebarCollectionsAsync()
        {
            var mostPopularArticles = await _articleService.GetMostPopularAsync(6);
            var editorChoiceArticles = await _articleService.GetEditorChoiceAsync(3);

            return (mostPopularArticles, editorChoiceArticles);
        }

        private async Task<HomeViewModel> BuildSearchViewModelAsync(string searchTerm)
        {
            var searchResults = await _articleService.SearchArticlesAsync(searchTerm);
            var weather = await _weatherService.GetWeatherAsync();
            var categories = await _categoryService.GetAllAsync();
            var sidebarCollections = await GetSidebarCollectionsAsync();

            return new HomeViewModel
            {
                SearchResults = searchResults,
                IsSearch = true,
                SearchTerm = searchTerm,
                MostPopularArticles = sidebarCollections.MostPopularArticles,
                EditorChoiceArticles = sidebarCollections.EditorChoiceArticles,
                Categories = categories,
                Weather = weather?.ToWeatherViewModel()
            };
        }

        private async Task<HomeViewModel> BuildDefaultViewModelAsync()
        {
            var (hasSubscription, preferredCategoryIds, preferredAuthorIds) = await GetUserPreferencesAsync();
            var latestArticles = await _articleService.GetLatestAsync(6);
            var sidebarCollections = await GetSidebarCollectionsAsync();
            var categories = await _categoryService.GetAllAsync();
            var weather = await _weatherService.GetWeatherAsync();

            IEnumerable<ArticleSummaryViewModel> prioritizedArticles = [];
            if (preferredCategoryIds.Any() || preferredAuthorIds.Any())
            {
                prioritizedArticles = await _articleService.GetAllArticlesSortedByPreferencesAsync(
                    preferredCategoryIds,
                    preferredAuthorIds,
                    4);
            }

            return new HomeViewModel
            {
                LatestArticles = latestArticles,
                MostPopularArticles = sidebarCollections.MostPopularArticles,
                EditorChoiceArticles = sidebarCollections.EditorChoiceArticles,
                PrioritizedArticles = prioritizedArticles,
                Categories = categories,
                HasActiveSubscription = hasSubscription,
                Weather = weather?.ToWeatherViewModel()
            };
        }

        private async Task<(bool HasSubscription, List<int> CategoryIds, List<string> AuthorIds)> GetUserPreferencesAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return (false, [], []);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return (false, [], []);
            }

            var hasSubscription = await _subscriptionService.HasActiveSubscriptionAsync(userId);
            var preferences = await _newsletterService.GetPreferencesAsync(userId);
            if (preferences == null)
            {
                return (hasSubscription, [], []);
            }

            var categoryIds = string.IsNullOrWhiteSpace(preferences.SelectedCategoryIds)
                ? []
                : preferences.SelectedCategoryIds.Split(',').Select(int.Parse).ToList();
            var authorIds = string.IsNullOrWhiteSpace(preferences.SelectedAuthorIds)
                ? []
                : preferences.SelectedAuthorIds.Split(',').ToList();

            return (hasSubscription, categoryIds, authorIds);
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

        public async Task<IActionResult> WeatherWidget(bool detailed = false)
        {
            return ViewComponent("WeatherCardVC", new { detailed });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SubscriptionAnalytics()
        {
            var stats = await _subscriptionAnalyticsService.GetDashboardStatsAsync();
            return View(stats);
        }
    }
}
