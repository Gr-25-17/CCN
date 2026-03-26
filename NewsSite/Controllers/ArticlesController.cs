using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers;

public class ArticlesController : Controller
{
    private readonly IArticleService _articleService;

    public ArticlesController(IArticleService articleService)
    {
        _articleService = articleService;
    }

    public async Task<IActionResult> Details(string slug)
    {
        if (string.IsNullOrEmpty(slug))
        {
            return NotFound();
        }

        var article = await _articleService.GetBySlugAsync(slug);

        if (article == null)
        {
            return NotFound();
        }

        return View(article);
    }
}