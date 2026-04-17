using Azure.Data.Tables;


namespace NewsSite.Services.Implementations
{
    public class GoldService
    {

        private readonly TableClient _tableClient;

        public GoldService(IConfiguration config)
        {
         
            var connectionString = config["AzureWebJobsStorage"];
            _tableClient = new TableClient(connectionString, "GoldPrices");
        }

        public async Task<List<GoldPrice>> GetLatestPricesAsync(int count = 7)
        {
         
            var results = _tableClient.QueryAsync<GoldPrice>(
                filter: "PartitionKey eq 'Gold'",
                maxPerPage: count
            ).Take(count);

            var list = new List<GoldPrice>();
            await foreach (var entity in results)
            {
                list.Add(entity);
            }
            return list;
        }
    }
}
