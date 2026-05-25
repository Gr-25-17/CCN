using System.Globalization;
using Azure;
using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class GoldService(IConfiguration config, ILogger<GoldService> logger) : IGoldService
{
    private const string PartitionKey = "Gold";
    private const string TimeBucketFormat = "yyyyMMddHH";

    private readonly TableClient _tableClient = new(
        config.GetConnectionString("AzureWebJobsStorage") ?? config["AzureWebJobsStorage"],
        "GoldPrices");

    public async Task<List<GoldPrice>> GetLatestPricesAsync(int count = 7)
    {
        var list = new List<GoldPrice>();

        try
        {
            var results = _tableClient.QueryAsync<GoldPrice>(
                filter: $"PartitionKey eq '{PartitionKey}'"
            );

            await foreach (var entity in results)
            {
                list.Add(entity);
            }
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Table Storage error fetching gold prices.");
            return [];
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "General error fetching gold prices. Check internet connection.");
            return [];
        }

        var ordered = list
            .Select(item => new { Item = item, Date = ParseSortableDateUtc(item.RowKey) })
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.Item.Timestamp)
            .Select(x => x.Item)
            .Take(count)
            .ToList();

        for (var i = 0; i < ordered.Count - 1; i++)
        {
            var current = ordered[i];
            var previous = ordered
                .Skip(i + 1)
                .FirstOrDefault(x => x.Close > 0 && Math.Abs(x.Close - current.Close) > 0.0001d);

            if (previous is not null)
            {
                current.PercentChange = ((current.Close - previous.Close) / previous.Close) * 100;
            }
        }

        return ordered;
    }

    private static DateTime ParseSortableDateUtc(string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(rowKey))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(
                rowKey,
                TimeBucketFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var bucketDate))
        {
            return bucketDate;
        }

        if (long.TryParse(rowKey, out var inverseTicks))
        {
            var ticks = DateTime.MaxValue.Ticks - inverseTicks;
            if (ticks >= 0 && ticks <= DateTime.MaxValue.Ticks)
            {
                return new DateTime(ticks, DateTimeKind.Utc);
            }
        }

        return DateTime.MinValue;
    }
}
