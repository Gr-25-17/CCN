using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterSender.Models;

namespace NewsletterSender.Services;

/// <summary>
/// Logs newsletter delivery attempts and results to Azure Table Storage.
/// </summary>
public class DeliveryLogger
{
    private readonly TableClient _tableClient;
    private readonly ILogger _logger;

    public DeliveryLogger(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var connectionString = config["AzureWebJobsStorage"];
        _tableClient = new TableClient(connectionString, "NewsletterDeliveryLog");
        _logger = loggerFactory.CreateLogger<DeliveryLogger>();
    }

    /// <summary>
    /// Logs a delivery attempt to the DeliveryLog table.
    /// </summary>
    public async Task LogDeliveryAsync(string newsletterId, string email, string userId, DateTime sentAt, string status, string? errorMessage = null)
    {
        try
        {
            await _tableClient.CreateIfNotExistsAsync();

            // Use a row key that sorts by time in descending order for efficient queries
            var rowKey = (DateTime.MaxValue.Ticks - sentAt.Ticks).ToString("d19");

            var entity = new DeliveryLogTableEntity
            {
                PartitionKey = newsletterId,
                RowKey = rowKey,
                NewsleterId = newsletterId,
                Email = email,
                UserId = userId,
                SentAt = sentAt,
                Status = status,
                ErrorMessage = errorMessage ?? string.Empty
            };

            await _tableClient.UpsertEntityAsync(entity);

            _logger.LogInformation($"Logged delivery for {email}: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error logging delivery: {ex.Message}");
            // Don't throw - delivery logging failure should not stop the process
        }
    }

    /// <summary>
    /// Retrieves delivery logs for a specific newsletter.
    /// </summary>
    public async Task<List<DeliveryLog>> GetDeliveryLogsAsync(string newsletterId)
    {
        var logs = new List<DeliveryLog>();

        try
        {
            var query = _tableClient.QueryAsync<DeliveryLogTableEntity>(
                filter: $"PartitionKey eq '{newsletterId}'"
            );

            await foreach (var entity in query)
            {
                logs.Add(entity.ToDeliveryLog());
            }

            _logger.LogInformation($"Retrieved {logs.Count} delivery logs for newsletter {newsletterId}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error retrieving delivery logs: {ex.Message}");
        }

        return logs;
    }

    /// <summary>
    /// Gets delivery statistics for a newsletter (sent, failed, bounced).
    /// </summary>
    public async Task<(int Sent, int Failed, int Bounced)> GetDeliveryStatsAsync(string newsletterId)
    {
        var logs = await GetDeliveryLogsAsync(newsletterId);
        return (
            Sent: logs.Count(l => l.Status == "Sent"),
            Failed: logs.Count(l => l.Status == "Failed"),
            Bounced: logs.Count(l => l.Status == "Bounced")
        );
    }

    /// <summary>
    /// Table Storage entity model for DeliveryLog.
    /// </summary>
    private class DeliveryLogTableEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public Azure.ETag ETag { get; set; }

        public string NewsleterId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public DeliveryLog ToDeliveryLog() => new()
        {
            NewsleterId = NewsleterId,
            Email = Email,
            UserId = UserId,
            SentAt = SentAt,
            Status = Status,
            ErrorMessage = string.IsNullOrEmpty(ErrorMessage) ? null : ErrorMessage
        };
    }
}
