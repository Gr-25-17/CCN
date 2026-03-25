using Microsoft.AspNetCore.Mvc;
using NewsSite.Models;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Diagnostics;

namespace NewsSite.Controllers
{
    public class HomeController : Controller
    {
        private readonly IArticleService _articleService;
        private readonly ICategoryService _categoryService;

        public HomeController(IArticleService articleService, ICategoryService categoryService)
        {
            _articleService = articleService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
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