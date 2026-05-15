using Microsoft.AspNetCore.Mvc;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/archive")]
    public class ArchiveApiController(IArticleRepository articleRepository) : ControllerBase
    {
        [HttpGet("articles-to-archive")]
        public async Task<IActionResult> GetArticlesToArchive([FromQuery] int days = 30)
        {
            var articles = await articleRepository.GetArticlesToArchiveAsync(days);
            return Ok(articles.Select(a => new { a.Id, a.Title, a.CreatedAt }));
        }

        [HttpPost("archive-articles")]
        public async Task<IActionResult> ArchiveArticles([FromBody] List<int> ids)
        {
            if (ids == null || ids.Count == 0) return BadRequest("No article IDs provided");
            await articleRepository.ArchiveArticlesAsync(ids);
            return Ok($"Archived {ids.Count} articles");
        }
    }
}