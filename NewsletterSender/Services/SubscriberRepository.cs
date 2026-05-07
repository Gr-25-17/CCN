using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterSender.Models;

namespace NewsletterSender.Services;

/// <summary>
/// Repository to load and manage subscribers from Azure Table Storage.
/// </summary>
public class SubscriberRepository
{
    private readonly TableClient _tableClient;
    private readonly ILogger _logger;

    public SubscriberRepository(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var connectionString = config["AzureWebJobsStorage"];
        _tableClient = new TableClient(connectionString, "Subscribers");
        _logger = loggerFactory.CreateLogger<SubscriberRepository>();
    }

    /// <summary>
    /// Retrieves all active subscribers from the Subscribers table.
    /// </summary>
    public async Task<List<Subscriber>> GetActiveSubscribersAsync()
    {
        var subscribers = new List<Subscriber>();

        try
        {
            await _tableClient.CreateIfNotExistsAsync();

            // Query for IsActive = true
            var query = _tableClient.QueryAsync<SubscriberTableEntity>(
                filter: "IsActive eq true"
            );

            await foreach (var entity in query)
            {
                subscribers.Add(entity.ToSubscriber());
            }

            _logger.LogInformation($"Retrieved {subscribers.Count} active subscribers");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving subscribers: {ex.Message}");
            throw;
        }

        return subscribers;
    }

    /// <summary>
    /// Updates LastSentAt timestamp for a batch of subscribers.
    /// </summary>
    public async Task UpdateLastSentAtAsync(List<string> userIds, DateTime sentAt)
    {
        try
        {
            foreach (var userId in userIds)
            {
                var entity = await _tableClient.GetEntityAsync<SubscriberTableEntity>("Subscribers", userId);
                if (entity.Value != null)
                {
                    entity.Value.LastSentAt = sentAt;
                    await _tableClient.UpdateEntityAsync(entity.Value, entity.Value.ETag);
                }
            }

    /// <summary>
    /// Upserts a subscriber entity into the Subscribers table.
    /// </summary>
    public async Task UpsertSubscriberAsync(Subscriber subscriber)
    {
        try
        {
            var entity = new SubscriberTableEntity
            {
                PartitionKey = "Subscribers",
                RowKey = subscriber.UserId ?? Guid.NewGuid().ToString(),
                UserId = subscriber.UserId ?? string.Empty,
                Email = subscriber.Email ?? string.Empty,
                FirstName = subscriber.FirstName ?? string.Empty,
                LastName = subscriber.FirstName ?? string.Empty,
                PreferredCategoryIds = subscriber.PreferredCategoryIds ?? string.Empty,
                IsActive = subscriber.IsActive,
                LastSentAt = subscriber.LastSentAt
            };

            await _tableClient.UpsertEntityAsync(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error upserting subscriber {UserId}", subscriber.UserId);
            throw;
        }
    }

            _logger.LogInformation($"Updated LastSentAt for {userIds.Count} subscribers");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating LastSentAt: {ex.Message}");
        }
    }

    /// <summary>
    /// Table Storage entity model for Subscriber.
    /// </summary>
    private class SubscriberTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "Subscribers";
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }

        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PreferredCategoryIds { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Locale { get; set; } = "en-US";
        public DateTime LastSentAt { get; set; }
        public string TemplateVersion { get; set; } = "1.0";

        public Subscriber ToSubscriber() => new()
        {
            UserId = UserId,
            Email = Email,
            FirstName = FirstName,
            LastName = LastName,
            PreferredCategoryIds = PreferredCategoryIds,
            IsActive = IsActive,
            Locale = Locale,
            LastSentAt = LastSentAt,
            TemplateVersion = TemplateVersion
        };
    }
}
