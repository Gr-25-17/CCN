using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GoldMarketTimer(
    ILogger<GoldMarketTimer> logger,
    StockMarketService stockService,
    IConfiguration configuration)
{
    [Function(nameof(GoldMarketTimer))]
    public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("Gold Fetcher started at: {Time}", DateTime.Now);

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

        var entity = new GoldPrice
        {
            PartitionKey = "Gold",
            RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"),
            Close = goldData.Close,
            PrevClose = goldData.PrevClose,
            PercentChange = goldData.PercentChange
        };

        await tableClient.UpsertEntityAsync(entity);
        logger.LogInformation("Successfully saved new gold price: {Close}", entity.Close);

        var allGoldEntries = new List<GoldPrice>();
        await foreach (var page in tableClient.QueryAsync<GoldPrice>(x => x.PartitionKey == "Gold").AsPages())
        {
            allGoldEntries.AddRange(page.Values);
        }

        if (allGoldEntries.Count > 10)
        {
            var entitiesToDelete = allGoldEntries.OrderBy(x => x.RowKey).Skip(10).ToList();

            var deleteTasks = entitiesToDelete.Select(oldPrice =>
                tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey));

            await Task.WhenAll(deleteTasks);

            foreach (var oldPrice in entitiesToDelete)
            {
                await tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey);
                logger.LogInformation("Deleted old record: {RowKey}", oldPrice.RowKey);
            }
        }
    }
}