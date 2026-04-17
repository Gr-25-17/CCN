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

            if (ViewBag.IsLocked && !string.IsNullOrEmpty(article.Content))
            {
                var paragraphs = article.Content.Split("</p>", StringSplitOptions.RemoveEmptyEntries);

                int paragraphsToKeep = Math.Min(2, paragraphs.Length);

                string truncatedContent = "";
                for (int i = 0; i < paragraphsToKeep; i++)
                {
                    truncatedContent += paragraphs[i] + "</p>";
                }

                article.Content = truncatedContent;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.HasLiked = userId != null && await articleService.HasUserLikedArticleAsync(article.Id, userId);

            return View(article);
        }

        [HttpPost, Authorize,ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int articleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var result = await articleService.ToggleLikeAsync(articleId, userId);
            return Json(new { success = true, isLiked = result.IsLiked, likesCount = result.LikesCount });
        }
    }
}