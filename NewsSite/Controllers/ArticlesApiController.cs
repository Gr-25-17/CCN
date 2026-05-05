using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers;

/// <summary>
/// API endpoints for newsletter service to fetch personalized articles.
/// Used by NewsletterSender Azure Function App.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ArticlesApiController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly ILogger<ArticlesApiController> _logger;

    public ArticlesApiController(IArticleService articleService, ILogger<ArticlesApiController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the latest articles for newsletter (simple list, no paging).
    /// </summary>
    [HttpGet("latest")]
    public async Task<ActionResult<List<NewsletterArticleDto>>> GetLatestArticles([FromQuery] int count = 5)
    {
        try
        {
            var articles = await _articleService.GetLatestAsync(count);
            var dtos = articles.Select(a => new NewsletterArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Slug = a.Slug,
                CategoryName = a.CategoryName,
                AuthorName = a.AuthorName,
                CreatedAt = a.CreatedAt,
                IsPremium = a.IsPremium
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching latest articles: {ex.Message}");
            return StatusCode(500, "Error fetching articles");
        }
    }

    /// <summary>
    /// Gets the latest articles for specified category IDs.
    /// </summary>
    [HttpGet("latest-by-categories")]
    public async Task<ActionResult<List<NewsletterArticleDto>>> GetLatestByCategories([FromQuery] string categories, [FromQuery] int count = 5)
    {
        try
        {
            // Parse comma-separated category IDs
            var categoryIds = categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();

            if (!categoryIds.Any())
            {
                return BadRequest("No valid category IDs provided");
            }

            var articles = await _articleService.GetLatestByCategoryIdsAsync(categoryIds, count);
            var dtos = articles.Select(a => new NewsletterArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Slug = a.Slug,
                CategoryName = a.CategoryName,
                AuthorName = a.AuthorName,
                CreatedAt = a.CreatedAt,
                IsPremium = a.IsPremium
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching articles by categories: {ex.Message}");
            return StatusCode(500, "Error fetching articles");
        }
    }

    /// <summary>
    /// Gets editor's choice articles (general).
    /// </summary>
    [HttpGet("editor-choice")]
    public async Task<ActionResult<List<NewsletterArticleDto>>> GetEditorChoice([FromQuery] int count = 3)
    {
        try
        {
            var articles = await _articleService.GetEditorChoiceAsync(count);
            var dtos = articles.Select(a => new NewsletterArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Slug = a.Slug,
                CategoryName = a.CategoryName,
                AuthorName = a.AuthorName,
                CreatedAt = a.CreatedAt,
                IsPremium = a.IsPremium
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching editor's choice articles: {ex.Message}");
            return StatusCode(500, "Error fetching articles");
        }
    }

    /// <summary>
    /// Gets editor's choice articles for specified category IDs.
    /// </summary>
    [HttpGet("editor-choice-by-categories")]
    public async Task<ActionResult<List<NewsletterArticleDto>>> GetEditorChoiceByCategories([FromQuery] string categories, [FromQuery] int count = 3)
    {
        try
        {
            var categoryIds = categories
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();

            if (!categoryIds.Any())
            {
                return BadRequest("No valid category IDs provided");
            }

            var articles = await _articleService.GetEditorChoiceByCategoryIdsAsync(categoryIds, count);
            var dtos = articles.Select(a => new NewsletterArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Slug = a.Slug,
                CategoryName = a.CategoryName,
                AuthorName = a.AuthorName,
                CreatedAt = a.CreatedAt,
                IsPremium = a.IsPremium
            }).ToList();

            return Ok(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching editor's choice articles by categories: {ex.Message}");
            return StatusCode(500, "Error fetching articles");
        }
    }
}

/// <summary>
/// DTO for newsletter articles (lightweight model for API responses).
/// </summary>
public class NewsletterArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPremium { get; set; }
}
