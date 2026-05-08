using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GoldMarketTimer(
    ILoggerFactory loggerFactory, 
    StockMarketService stockService, 
    IConfiguration configuration)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GoldMarketTimer>();

    [Function(nameof(GoldMarketTimer))]
    public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Gold Fetcher started at: {Time}", DateTime.Now);

        var goldData = await stockService.GetGoldAsync();
        
        if (goldData is null)
        {
            _logger.LogWarning("No gold data returned from StockMarketService. Aborting update.");
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
            Close = (double)goldData.Close, 
            PrevClose = (double)goldData.PrevClose,
            PercentChange = (double)goldData.PercentChange
        };

        await tableClient.UpsertEntityAsync(entity);
        _logger.LogInformation("Successfully saved new gold price: {Close}", entity.Close);

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

            _logger.LogInformation("Cleaned up {Count} old gold records.", entitiesToDelete.Count);
        }
    }
}