using System.Net.Http.Json;
using CCN.Jobs.Functions.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CCN.Jobs.Functions;

public class StockMarketService(HttpClient httpClient, IConfiguration configuration, ILogger<StockMarketService> logger)
{
    public async Task<Top10?> GetGoldAsync()
    {
        var url = configuration["StockApiUrl"];
        if (string.IsNullOrEmpty(url))
        {
            logger.LogError("Missing configuration for StockApiUrl.");
            return null;
        }

        try
        {
            var result = await httpClient.GetFromJsonAsync<StockPrice>(url);
            return result?.Top10Stock?.FirstOrDefault(x => x.Symbol == "GC=F" || x.Name == "Gold");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch stock summary from external API.");
            return null;
        }
    }
}
