using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using CCN.Jobs.Functions.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace CCN.Jobs.Functions.Functions;

public class ArchiveFunction
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ArchiveFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<ArchiveFunction>();
        _httpClient = httpClientFactory.CreateClient();
        _baseUrl = configuration["NewsSiteBaseUrl"] ?? throw new InvalidOperationException("Missing NewsSiteBaseUrl configuration");
    }

    [Function("ArchiveFunction")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Archive function triggered at: {time}", DateTime.UtcNow);

        try
        {
            // Hämta artiklar som ska arkiveras
            var articlesToArchive = await _httpClient.GetFromJsonAsync<List<ArticleDto>>($"{_baseUrl.TrimEnd('/')}/api/archive/articles-to-archive?days=30");

            if (articlesToArchive == null || !articlesToArchive.Any())
            {
                _logger.LogInformation("No articles to archive");
                return;
            }

            // Spara i Azure Table Storage
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("Missing AzureWebJobsStorage configuration");

            var tableClient = new TableClient(connectionString, "ArchivedArticles");
            await tableClient.CreateIfNotExistsAsync();

            foreach (var article in articlesToArchive)
            {
                var entity = new ArchivedArticle
                {
                    RowKey = article.Id.ToString(),
                    ArticleId = article.Id,
                    Title = article.Title,
                    CreatedAt = article.CreatedAt,
                    ArchivedAt = DateTime.UtcNow
                };

                await tableClient.UpsertEntityAsync(entity);
            }

            _logger.LogInformation("Saved {count} articles to Table Storage", articlesToArchive.Count);

            // Arkivera artiklarna i NewsSite
            var ids = articlesToArchive.Select(a => a.Id).ToList();
            var response = await _httpClient.PostAsJsonAsync($"{_baseUrl.TrimEnd('/')}/api/archive/archive-articles", ids);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Archived {count} articles", ids.Count);
            }
            else
            {
                _logger.LogError("Failed to archive articles. Status: {status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error archiving articles");
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}

public class ArticleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}