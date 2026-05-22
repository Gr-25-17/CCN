using Azure.Data.Tables;
using CCN.Jobs.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCN.Jobs.Functions.Functions;

public class GoldMarketTimer(
    ILogger<GoldMarketTimer> logger,
    StockMarketService stockService,
    IConfiguration configuration)
{
    private const string PartitionKey = "Gold";
    private const string TimeBucketFormat = "yyyyMMddHH";
    private const int MaxStoredPoints = 10;

    [Function(nameof(GoldMarketTimer))]
    public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("Gold Fetcher started at: {Time}", DateTime.UtcNow);

        var goldData = await stockService.GetGoldAsync();
        if (goldData is null)
        {
            logger.LogWarning("No gold data returned from StockMarketService. Aborting update.");
            return;
        }

        var connectionString = configuration["AzureWebJobsStorage"]
            ?? throw new InvalidOperationException("Missing config: AzureWebJobsStorage");

        var tableClient = new TableClient(connectionString, "GoldPrices");
        await tableClient.CreateIfNotExistsAsync();

        var bucketTimeUtc = NormalizeToHourUtc(DateTime.UtcNow);
        var rowKey = bucketTimeUtc.ToString(TimeBucketFormat);

        var entity = new GoldPrice
        {
            PartitionKey = PartitionKey,
            RowKey = rowKey,
            Close = goldData.Close,
            PrevClose = goldData.PrevClose,
            PercentChange = goldData.PercentChange,
            Name = goldData.Name,
            Symbol = goldData.Symbol
        };

        await tableClient.UpsertEntityAsync(entity);
        logger.LogInformation("Upserted gold price for bucket {Bucket}: {Close}", rowKey, entity.Close);

        var allGoldEntries = new List<GoldPrice>();
        await foreach (var page in tableClient.QueryAsync<GoldPrice>(x => x.PartitionKey == PartitionKey).AsPages())
        {
            allGoldEntries.AddRange(page.Values);
        }

        if (allGoldEntries.Count <= MaxStoredPoints)
        {
            return;
        }

        var entitiesToDelete = allGoldEntries
            .OrderByDescending(x => ParseSortableDateUtc(x.RowKey))
            .Skip(MaxStoredPoints)
            .ToList();

        var deleteTasks = entitiesToDelete.Select(oldPrice => tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey));
        await Task.WhenAll(deleteTasks);

        foreach (var oldPrice in entitiesToDelete)
        {
            logger.LogInformation("Deleted old gold record: {RowKey}", oldPrice.RowKey);
        }
    }

    private static DateTime NormalizeToHourUtc(DateTime utcNow) =>
        new(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, 0, 0, DateTimeKind.Utc);

    private static DateTime ParseSortableDateUtc(string? rowKey)
    {
        if (string.IsNullOrWhiteSpace(rowKey))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(
                rowKey,
                TimeBucketFormat,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
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
