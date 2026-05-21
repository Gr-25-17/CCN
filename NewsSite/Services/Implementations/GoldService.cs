using Azure;
using Azure.Data.Tables;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class GoldService(IConfiguration config, ILogger<GoldService> logger) : IGoldService
{
    private readonly TableClient _tableClient = new(
        config.GetConnectionString("AzureWebJobsStorage") ?? config["AzureWebJobsStorage"],
        "GoldPrices");

    public async Task<List<GoldPrice>> GetLatestPricesAsync(int count = 7, string symbol = "Gold")
    {
        var list = new List<GoldPrice>();

        try
        {
            var normalizedSymbol = string.IsNullOrWhiteSpace(symbol) ? "Gold" : symbol.Trim();

            var results = _tableClient.QueryAsync<GoldPrice>(
                filter: $"PartitionKey eq '{normalizedSymbol}'",
                maxPerPage: count
            ).Take(count);

            await foreach (var entity in results)
            {
                list.Add(entity);
            }
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Azure Table Storage error fetching {Symbol} prices.", symbol);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "General error fetching {Symbol} prices. Check internet connection.", symbol);
        }

        return list;
    }
}
