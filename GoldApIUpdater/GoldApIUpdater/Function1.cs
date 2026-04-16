using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class Function1
{
    private readonly ILogger _logger;
    private readonly StockMarketService _stockService;
    private readonly IConfiguration _configuration;

    public Function1(ILoggerFactory loggerFactory, StockMarketService stockService, IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<Function1>();
        _stockService = stockService;
        _configuration = configuration; 
    }

    [Function("GoldUpdater")]
    public async Task Run([TimerTrigger("0 0 0 */2 * *")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Gold Fetcher started at: {DateTime.Now}");

       
        var goldData = await _stockService.GetGoldAsync();

        if (goldData != null)
        {
            string connectionString = _configuration["AzureWebJobsStorage"];

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("Connection string is missing!");
                return;
            }

            var tableClient = new TableClient(connectionString, "GoldPrices");
            await tableClient.CreateIfNotExistsAsync();

      
            var entity = new GoldPrice
            {
           
                RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"),
                Close = goldData.Close,
                PrevClose = goldData.PrevClose,
                PercentChange = goldData.PercentChange
            };

            await tableClient.UpsertEntityAsync(entity);

            var oldPrices = tableClient.Query<GoldPrice>().Skip(10).ToList();

            foreach (var oldPrice in oldPrices)
            {
                await tableClient.DeleteEntityAsync(oldPrice.PartitionKey, oldPrice.RowKey);
            }

            _logger.LogInformation($"Successfully saved Gold price: {goldData.Close}");
        }
    }
}