using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICategoryService _categoryService;
        private readonly IWeatherService _weatherService;

        public CategoryController(IArticleService articleService, ICategoryService categoryService, IWeatherService weatherService)
        {
            _articleService = articleService;
            _categoryService = categoryService;
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index(string slug)
        {
            var categories = await _categoryService.GetAllAsync();

            var category = categories.FirstOrDefault(c => c.Name.ToLower() == slug.ToLower());

            if (category == null)
            {
                return NotFound();
            }

            if (category.Name.Equals("Weather", StringComparison.OrdinalIgnoreCase))
            {
                var weather = await _weatherService.GetWeatherAsync();
                ViewBag.Weather = weather.ToWeatherViewModel();
            }
            var articles = await _articleService.GetByCategoryAsync(category.Id, 1, 10);

            ViewBag.Category = category;

            var premiumArticles = await _articleService.GetPremiumByCategoryAsync(category.Id, 5);
            ViewBag.PremiumArticles = premiumArticles;

            return View(articles);
        }
    }
}