using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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

            // Pattern matching (is { }) säkerställer att vi inte får NullReferenceExceptions
            if (result?.Top10Stock is { } stocks)
            {
                return stocks.FirstOrDefault(x => x.Symbol == "GC=F" || x.Name == "Gold");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch stock summary from external API.");
        }

        return null;
    }
}