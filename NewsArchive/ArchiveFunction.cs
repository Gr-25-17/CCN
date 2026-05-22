using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsArchive.Models;
using System.Net.Http.Json;

namespace NewsArchive;

public class ArchiveFunction
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    private const string BaseUrl = "https://ccnews-fhdvd8hqcdc0akgx.swedencentral-01.azurewebsites.net";

    public ArchiveFunction(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
    {
        _logger = loggerFactory.CreateLogger<ArchiveFunction>();
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("ArchiveFunction")]
    public async Task Run([TimerTrigger("0 0 2 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Archive function triggered at: {time}", DateTime.UtcNow);

        try
        {
            // Hämta artiklar som ska arkiveras
            var articlesToArchive = await _httpClient.GetFromJsonAsync<List<ArticleDto>>($"{BaseUrl}/api/archive/articles-to-archive?days=30");

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
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/api/archive/archive-articles", ids);

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