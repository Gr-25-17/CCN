using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICategoryService _categoryService;

        public CategoryController(IArticleService articleService, ICategoryService categoryService)
        {
            _articleService = articleService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(string slug)
        {
            var categories = await _categoryService.GetAllAsync();

            var category = categories.FirstOrDefault(c => c.Name.ToLower() == slug.ToLower());

            if (category == null)
            {
                return NotFound();
            }

            var articles = await _articleService.GetByCategoryAsync(category.Id, 1, 10);

            ViewBag.Category = category;

            return View(articles);
        }
    }
}