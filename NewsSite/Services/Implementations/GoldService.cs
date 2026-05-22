using Azure;
using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class GoldService(IConfiguration config, ILogger<GoldService> logger) : IGoldService
{
    private readonly TableClient _tableClient = new(
        config.GetConnectionString("AzureWebJobsStorage") ?? config["AzureWebJobsStorage"],
        "GoldPrices");

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
            logger.LogError(ex, "Azure Table Storage error fetching gold prices.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "General error fetching gold prices. Check internet connection.");
        }

        return list;
    }
}
