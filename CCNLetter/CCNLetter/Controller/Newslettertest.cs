using CCNLetter.Models;
using CCNLetter.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace CCNLetter.Controller
{
    internal class Newslettertest
    {
        private readonly IEmailService _emailService;
        private readonly INewsletterContentService _newsletterContentService;
        private readonly ILogger<Newslettertest> _logger;
        private readonly HttpClient _httpClient;

        // Toggle between local and HTTP mode
        private readonly bool UseLocalMode = true; // Set to false for production HTTP mode

        public Newslettertest(
            IEmailService emailService,
            INewsletterContentService newsletterContentService,
            ILogger<Newslettertest> logger,
            IHttpClientFactory httpClientFactory)
        {
            _emailService = emailService;
            _newsletterContentService = newsletterContentService;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("SendNewsletter")]
        public async Task Run([TimerTrigger("0 0 8 * * 1")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Newsletter function started at: {DateTime.Now}");

            try
            {
                List<Article> articles;
                string htmlContent;
                string newsletterTitle = $"CCN Veckans Nyheter - {DateTime.Now:yyyy-MM-dd}";

                if (UseLocalMode)
                {
                    // LOCAL MODE: Use your existing services (perfect for testing)
                    _logger.LogInformation("Using LOCAL mode with test articles");
                    articles = await _newsletterContentService.GetFeaturedArticlesAsync(5);
                    htmlContent = await _newsletterContentService.GenerateNewsletterHtmlAsync(articles, newsletterTitle);
                }
                else
                {
                    // PRODUCTION MODE: Call your MVC app's API
                    _logger.LogInformation("Using PRODUCTION mode with HTTP calls");

                    var baseUrl = Environment.GetEnvironmentVariable("Data Source=app.db")
                        ?? "https://ccnstorage.blob.core.windows.net/";

                    // Get articles from MVC app
                    articles = await _httpClient.GetFromJsonAsync<List<Article>>(
                        $"{baseUrl}/api/newsletter/featured-articles?count=5");

                    if (articles == null || !articles.Any())
                    {
                        _logger.LogWarning("No articles found from MVC app");
                        return;
                    }

                    // Get HTML from MVC app
                    var request = new NewsletterRequest
                    {
                        Articles = articles,
                        Title = newsletterTitle
                    };

                    var response = await _httpClient.PostAsJsonAsync(
                        $"{baseUrl}/api/newsletter/generate-html",
                        request);

                    htmlContent = await response.Content.ReadAsStringAsync();
                }

                // Send email (works the same in both modes)
                string testEmail = "johan.liljeberg90@gmail.com";
                var result = await _emailService.SendEmailAsync(testEmail, newsletterTitle, htmlContent);

                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogInformation($"Newsletter sent successfully to {testEmail}");
                    foreach (var article in articles)
                    {
                        _logger.LogInformation($"Included: {article.Title}");
                    }
                }
                else
                {
                    _logger.LogError($"Email sending failed: {result}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Newsletter function failed");
            }
        }
    }

    public class NewsletterRequest
    {
        public List<Article> Articles { get; set; } = new();
        public string Title { get; set; } = string.Empty;
    }
}
