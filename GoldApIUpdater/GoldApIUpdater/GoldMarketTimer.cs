using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

// C# 12 Primary Constructor för renare Dependency Injection
public class GoldMarketTimer(
    ILoggerFactory loggerFactory, 
    StockMarketService stockService, 
    IConfiguration configuration)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<GoldMarketTimer>();

    [Function(nameof(GoldMarketTimer))]
    public async Task Run([TimerTrigger("0 0 0 */2 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Gold Fetcher started at: {Time}", DateTime.Now);

        var goldData = await stockService.GetGoldAsync();
        
        // Guard clause med loggning istället för silent return
        if (goldData is null)
        {
            _logger.LogWarning("No gold data returned from StockMarketService. Aborting update.");
            return;
        }

        var connectionString = configuration["AzureWebJobsStorage"] 
            ?? throw new InvalidOperationException("Missing config: AzureWebJobsStorage");
            
        var tableClient = new TableClient(connectionString, "GoldPrices");
        await tableClient.CreateIfNotExistsAsync();

        // 1. Spara det nya priset
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

        // 2. Hämta asynkront och städa upp gamla poster (behåll de 10 nyaste)
        var allGoldEntries = new List<GoldPrice>();
        await foreach (var page in tableClient.QueryAsync<GoldPrice>(x => x.PartitionKey == "Gold").AsPages())
        {
            allGoldEntries.AddRange(page.Values);
        }

        if (allGoldEntries.Count > 10)
        {
            // Reverse-ticks gör att OrderBy(RowKey) lägger de NYASTE först.
            // .Skip(10) hoppar över de 10 nyaste, och returnerar de äldre posterna.
            var entitiesToDelete = allGoldEntries.OrderBy(x => x.RowKey).Skip(10);

            foreach (var oldPrice in entitiesToDelete)
            {
                // Säker radering via RowKey och PartitionKey
                await tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey);
                _logger.LogInformation("Deleted old record: {RowKey}", oldPrice.RowKey);
            }
        }
    }
}