using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterSender.Models;

namespace NewsletterSender.Services;

/// <summary>
/// Builds personalized newsletter content for each subscriber.
/// Integrates with ArticleServiceClient to fetch relevant articles based on subscriber preferences.
/// </summary>
public class NewsletterBuilder
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly ArticleServiceClient _articleClient;

    public NewsletterBuilder(IConfiguration config, ILoggerFactory loggerFactory, ArticleServiceClient articleClient)
    {
        _configuration = config;
        _logger = loggerFactory.CreateLogger<NewsletterBuilder>();
        _articleClient = articleClient;
    }

    /// <summary>
    /// Builds a personalized newsletter for a subscriber based on their preferences.
    /// </summary>
    public async Task<NewsletterContent> BuildPersonalizedNewsletterAsync(Subscriber subscriber)
    {
        try
        {
            // Parse preferred category IDs from comma-separated string
            var categoryIds = ParseCategoryIds(subscriber.PreferredCategoryIds);

            // TODO: Fetch articles for this subscriber using IArticleService
            // For now, placeholder articles
            var topArticles = await FetchTopArticlesForCategoriesAsync(categoryIds);
            var editorPickArticles = await FetchEditorPickArticlesAsync(categoryIds);

            // Build HTML content
            var htmlBody = BuildHtmlContent(subscriber, topArticles, editorPickArticles);
            var subject = $"Your Weekly News Digest - {DateTime.Now:MMMM dd, yyyy}";

            _logger.LogInformation($"Built newsletter for subscriber {subscriber.Email}");

            return new NewsletterContent
            {
                Subject = subject,
                HtmlBody = htmlBody
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error building newsletter for {subscriber.Email}: {ex.Message}");
            throw;
        }
    }

    private List<int> ParseCategoryIds(string preferredCategoryIds)
    {
        if (string.IsNullOrEmpty(preferredCategoryIds))
            return new List<int>();

        return preferredCategoryIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
            .Where(id => id > 0)
            .ToList();
    }

    private async Task<List<NewsletterArticle>> FetchTopArticlesForCategoriesAsync(List<int> categoryIds)
    {
        // Call ArticleServiceClient to fetch latest articles for subscriber's preferred categories
        var articles = await _articleClient.GetLatestArticlesByCategoriesAsync(categoryIds, count: 5);
        _logger.LogInformation($"Fetched {articles.Count} top articles for categories");
        return articles;
    }

    private async Task<List<NewsletterArticle>> FetchEditorPickArticlesAsync(List<int> categoryIds)
    {
        // Call ArticleServiceClient to fetch editor's choice articles for subscriber's preferred categories
        var articles = await _articleClient.GetEditorChoiceArticlesByCategoriesAsync(categoryIds, count: 3);
        _logger.LogInformation($"Fetched {articles.Count} editor's choice articles");
        return articles;
    }

    private string BuildHtmlContent(Subscriber subscriber, List<NewsletterArticle> topArticles, List<NewsletterArticle> editorPickArticles)
    {
        var baseUrl = _configuration["NewsletterBaseUrl"] ?? "https://newssite.com";
        var unsubscribeToken = GenerateUnsubscribeToken(subscriber.UserId);

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{System.Web.HttpUtility.HtmlEncode("Weekly News Digest")}</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; }}
        .header {{ background-color: #007bff; color: white; padding: 20px; text-align: center; }}
        .section {{ margin: 20px 0; }}
        .article {{ border: 1px solid #ddd; padding: 15px; margin: 10px 0; }}
        .article h3 {{ margin: 0 0 10px 0; }}
        .article a {{ color: #007bff; text-decoration: none; }}
        .footer {{ text-align: center; font-size: 12px; color: #666; margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; }}
        .unsubscribe {{ color: #0066cc; text-decoration: underline; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Your Weekly News Digest</h1>
            <p>Hi {System.Web.HttpUtility.HtmlEncode(subscriber.FirstName)}, here's what's new this week!</p>
        </div>

        <div class=""section"">
            <h2>Top Stories</h2>
            {string.Join("", topArticles.Select(a => BuildArticleHtml(a, baseUrl)))}
        </div>

        {(editorPickArticles.Any() ? $@"
        <div class=""section"">
            <h2>⭐ Editor's Picks</h2>
            {string.Join("", editorPickArticles.Select(a => BuildArticleHtml(a, baseUrl)))}
        </div>
        " : "")}

        <div class=""footer"">
            <p>
                <a href=""{baseUrl}"" style=""color: #007bff; text-decoration: none;"">Visit Website</a> |
                <a href=""{baseUrl}/account/preferences"" style=""color: #007bff; text-decoration: none;"">Update Preferences</a> |
                <a href=""{baseUrl}/api/newsletter/unsubscribe?token={System.Web.HttpUtility.UrlEncode(unsubscribeToken)}"" class=""unsubscribe"">Unsubscribe</a>
            </p>
            <p>&copy; 2025 NewsletterSender. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
";
        return html;
    }

    private string BuildArticleHtml(NewsletterArticle article, string baseUrl)
    {
        var premiumBadge = article.IsPremium ? " | ⭐ Premium" : "";
        return $@"
<div class=""article"">
    <h3><a href=""{baseUrl}/articles/{System.Web.HttpUtility.UrlEncode(article.Slug)}"">{System.Web.HttpUtility.HtmlEncode(article.Title)}</a></h3>
    <p>{System.Web.HttpUtility.HtmlEncode(article.Summary)}</p>
    <small style=""color: #666;"">
        {System.Web.HttpUtility.HtmlEncode(article.CategoryName)} | By {System.Web.HttpUtility.HtmlEncode(article.AuthorName)} | {article.CreatedAt:MMMM dd, yyyy}{premiumBadge}
    </small>
</div>
";
    }

    private string GenerateUnsubscribeToken(string userId)
    {
        // TODO: Generate a secure unsubscribe token (e.g., using HMAC or JWT)
        // For now, use a simple approach
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{userId}:{DateTime.UtcNow.AddDays(30)}"));
    }
}
