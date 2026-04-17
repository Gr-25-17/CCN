using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class GoldMarketTimer
{
    private readonly ILogger _logger;
    private readonly StockMarketService _stockService;
    private readonly IConfiguration _configuration;

    public GoldMarketTimer(ILoggerFactory loggerFactory, StockMarketService stockService, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<GoldMarketTimer>();
        _stockService = stockService;
        _configuration = configuration; 
    }

    [Function("GoldUpdater")]//
    public async Task Run([TimerTrigger("0 0 0 */2 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Gold Fetcher started at: {DateTime.Now}");

        var goldData = await _stockService.GetGoldAsync();
        if (goldData == null) return;

        string connectionString = _configuration["AzureWebJobsStorage"];
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

   
        var allGoldEntries = tableClient.Query<GoldPrice>(x => x.PartitionKey == "Gold").ToList();

        if (allGoldEntries.Count > 10)
        {
           
            var entitiesToDelete = allGoldEntries.OrderBy(x => x.RowKey).SkipLast(10);

            foreach (var oldPrice in entitiesToDelete)
            {
                await tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey);
                _logger.LogInformation($"Deleted old record: {oldPrice.RowKey}");
            }
        }
    }
}