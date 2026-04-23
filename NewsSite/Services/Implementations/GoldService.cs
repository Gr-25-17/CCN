using Azure;
using Azure.Data.Tables;
using NewsSite.Services.Interfaces;


namespace NewsSite.Services.Implementations
{
    public class GoldService : IGoldService
    {
        private readonly TableClient _tableClient;
        private readonly ILogger<GoldService> _logger; 

        public GoldService(IConfiguration config, ILogger<GoldService> logger)
        {
            var connectionString = config["AzureWebJobsStorage"];
            _tableClient = new TableClient(connectionString, "GoldPrices");
            _logger = logger;
        }

        public async Task<List<GoldPrice>> GetLatestPricesAsync(int count = 7)
        {
            var list = new List<GoldPrice>();

            try
            {
                var results = _tableClient.QueryAsync<GoldPrice>(
                    filter: "PartitionKey eq 'Gold'",
                    maxPerPage: count
                ).Take(count);

                await foreach (var entity in results)
                {
                    list.Add(entity);
                }
            }
            catch (RequestFailedException ex)
            {

                _logger.LogError(ex, "Azure Table Storage error fetching gold prices.");
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "General error fetching gold prices. Check internet connection.");
            }

            return list; 
        }
    }
}
