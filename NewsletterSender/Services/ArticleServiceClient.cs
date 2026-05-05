using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using NewsletterSender.Models;

namespace NewsletterSender.Services;

/// <summary>
/// Client to call NewsSite API to fetch articles for newsletters.
/// Integrates with IArticleService via HTTP endpoints.
/// </summary>
public class ArticleServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger _logger;

    public ArticleServiceClient(HttpClient httpClient, IConfiguration config, ILoggerFactory loggerFactory)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = loggerFactory.CreateLogger<ArticleServiceClient>();
    }

    /// <summary>
    /// Fetches latest articles for given category IDs from the NewsSite API.
    /// </summary>
    public async Task<List<NewsletterArticle>> GetLatestArticlesByCategoriesAsync(List<int> categoryIds, int count = 5)
    {
        try
        {
            if (!categoryIds.Any())
            {
                // Fallback: get general latest articles
                return await GetLatestArticlesAsync(count);
            }

            // Call API endpoint (to be created in NewsSite)
            var url = $"{_config["NewsletterApiBaseUrl"]}/api/articles/latest-by-categories?categories={string.Join(",", categoryIds)}&count={count}";
            var articles = await _httpClient.GetFromJsonAsync<List<NewsletterArticle>>(url);

            _logger.LogInformation($"Fetched {articles?.Count ?? 0} articles for categories {string.Join(",", categoryIds)}");
            return articles ?? new List<NewsletterArticle>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching articles by categories: {ex.Message}");
            return new List<NewsletterArticle>();
        }
    }

    /// <summary>
    /// Fetches latest articles (general) from the NewsSite API.
    /// </summary>
    public async Task<List<NewsletterArticle>> GetLatestArticlesAsync(int count = 5)
    {
        try
        {
            var url = $"{_config["NewsletterApiBaseUrl"]}/api/articles/latest?count={count}";
            var articles = await _httpClient.GetFromJsonAsync<List<NewsletterArticle>>(url);

            _logger.LogInformation($"Fetched {articles?.Count ?? 0} latest articles");
            return articles ?? new List<NewsletterArticle>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching latest articles: {ex.Message}");
            return new List<NewsletterArticle>();
        }
    }

    /// <summary>
    /// Fetches editor's choice articles for given categories.
    /// </summary>
    public async Task<List<NewsletterArticle>> GetEditorChoiceArticlesByCategoriesAsync(List<int> categoryIds, int count = 3)
    {
        try
        {
            if (!categoryIds.Any())
            {
                return await GetEditorChoiceArticlesAsync(count);
            }

            var url = $"{_config["NewsletterApiBaseUrl"]}/api/articles/editor-choice-by-categories?categories={string.Join(",", categoryIds)}&count={count}";
            var articles = await _httpClient.GetFromJsonAsync<List<NewsletterArticle>>(url);

            _logger.LogInformation($"Fetched {articles?.Count ?? 0} editor's choice articles");
            return articles ?? new List<NewsletterArticle>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching editor's choice articles: {ex.Message}");
            return new List<NewsletterArticle>();
        }
    }

    /// <summary>
    /// Fetches editor's choice articles (general).
    /// </summary>
    public async Task<List<NewsletterArticle>> GetEditorChoiceArticlesAsync(int count = 3)
    {
        try
        {
            var url = $"{_config["NewsletterApiBaseUrl"]}/api/articles/editor-choice?count={count}";
            var articles = await _httpClient.GetFromJsonAsync<List<NewsletterArticle>>(url);

            _logger.LogInformation($"Fetched {articles?.Count ?? 0} editor's choice articles (general)");
            return articles ?? new List<NewsletterArticle>();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching editor's choice articles: {ex.Message}");
            return new List<NewsletterArticle>();
        }
    }
}
