using Azure.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsSite.Data;
using NewsSite.Models.Entities;

namespace NewsletterSender.Services;

/// <summary>
/// Seeder that populates the Subscribers table in Azure Storage from NewsSite database.
/// Reads users with active newsletter preferences and subscriptions.
/// </summary>
public class SubscriberSeeder
{
    private readonly TableClient _tableClient;
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger _logger;

    public SubscriberSeeder(IConfiguration config, ApplicationDbContext dbContext, ILoggerFactory loggerFactory)
    {
        var connectionString = config["AzureWebJobsStorage"];
        _tableClient = new TableClient(connectionString, "Subscribers");
        _dbContext = dbContext;
        _logger = loggerFactory.CreateLogger<SubscriberSeeder>();
    }

    /// <summary>
    /// Seeds Subscribers table from NewsSite database.
    /// Loads users who:
    /// - Have active subscriptions OR have newsletter preferences enabled
    /// - Have email addresses
    /// </summary>
    public async Task SeedSubscribersAsync()
    {
        try
        {
            await _tableClient.CreateIfNotExistsAsync();

            // Query newsletter preferences for active recipients
            var preferences = await _dbContext.NewsletterPreferences
                .Where(p => p.ReceiveNewsletter)
                .ToListAsync();

            _logger.LogInformation($"Found {preferences.Count} newsletter subscribers");

            if (!preferences.Any())
            {
                _logger.LogWarning("No newsletter subscribers found for seeding");
                return;
            }

            // Get all users in one query
            var userIds = preferences.Select(p => p.UserId).ToList();
            var users = await _dbContext.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            int seededCount = 0;
            foreach (var user in users)
            {
                try
                {
                    // Get newsletter preferences for this user
                    var prefs = preferences.FirstOrDefault(p => p.UserId == user.Id);
                    if (prefs == null || !prefs.ReceiveNewsletter)
                    {
                        continue;
                    }

                    // Create subscriber record
                    var entity = new SubscriberTableEntity
                    {
                        PartitionKey = "Subscribers",
                        RowKey = user.Id,
                        UserId = user.Id,
                        Email = user.Email ?? string.Empty,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        PreferredCategoryIds = prefs.SelectedCategoryIds ?? string.Empty,
                        IsActive = true,
                        Locale = "sv-SE", // Default to Swedish, can be customized per user
                        LastSentAt = prefs.LastSentDate ?? DateTime.MinValue,
                        TemplateVersion = "1.0"
                    };

                    // Skip if no email
                    if (string.IsNullOrEmpty(entity.Email))
                    {
                        _logger.LogWarning($"User {user.Id} has no email. Skipping.");
                        continue;
                    }

                    await _tableClient.UpsertEntityAsync(entity);
                    seededCount++;
                    _logger.LogInformation($"Seeded subscriber: {entity.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error seeding subscriber {user.Id}: {ex.Message}");
                }
            }

            _logger.LogInformation($"Successfully seeded {seededCount} subscribers");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error seeding subscribers: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Clears all subscribers from the table (for testing/resetting).
    /// </summary>
    public async Task ClearSubscribersAsync()
    {
        try
        {
            var entities = _tableClient.QueryAsync<SubscriberTableEntity>(
                filter: "PartitionKey eq 'Subscribers'"
            );

            await foreach (var entity in entities)
            {
                await _tableClient.DeleteEntityAsync(entity.PartitionKey, entity.RowKey);
            }

            _logger.LogInformation("Cleared all subscribers");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error clearing subscribers: {ex.Message}");
        }
    }

    /// <summary>
    /// Table Storage entity model for Subscriber.
    /// </summary>
    private class SubscriberTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
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
    }
}
