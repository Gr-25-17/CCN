using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace NewsSite.Controllers
{
    public class ArticlesController(IArticleService articleService) : Controller
    {
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return NotFound();

            var article = await articleService.GetBySlugAsync(slug);
            if (article == null) return NotFound();

            await articleService.IncrementViewCountAsync(article.Id);
            article.ViewsCount++;

            bool isAuthorized = User.IsInRole("Admin") ||
                                User.IsInRole("Editor") ||
                                User.IsInRole("Writer") ||
                                User.IsInRole("Subscriber");

            ViewBag.IsLocked = article.IsPremium && !isAuthorized;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.HasLiked = userId != null && await articleService.HasUserLikedArticleAsync(article.Id, userId);

            return View(article);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await articleService.ToggleLikeAsync(articleId, userId);

            return Json(new { success = true, isLiked = result.IsLiked, likesCount = result.LikesCount });
        }
    }
}