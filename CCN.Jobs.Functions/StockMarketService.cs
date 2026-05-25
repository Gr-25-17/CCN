using System.Globalization;
using System.Text.Json;
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
            using var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Stock API request failed with status code: {StatusCode}", response.StatusCode);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var document = await JsonDocument.ParseAsync(stream);

            if (!document.RootElement.TryGetProperty("top10", out var top10Array) || top10Array.ValueKind != JsonValueKind.Array)
            {
                logger.LogWarning("Stock API response does not contain a valid top10 array.");
                return null;
            }

            foreach (var item in top10Array.EnumerateArray())
            {
                var symbol = GetString(item, "symbol");
                var name = GetString(item, "name");

                if (!string.Equals(symbol, "GC=F", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(name, "Gold", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var close = GetDouble(item, "close") ?? GetDouble(item, "price") ?? 0;
                var prevClose = GetDouble(item, "prevClose") ?? 0;
                var percentChange = GetDouble(item, "percentChange") ?? 0;

                if (close <= 0 && prevClose > 0)
                {
                    logger.LogWarning("Gold close was non-positive from API, falling back to prevClose and neutral percent change. Symbol: {Symbol}, Name: {Name}, Close: {Close}, PrevClose: {PrevClose}", symbol, name, close, prevClose);
                    close = prevClose;
                    percentChange = 0;
                }

                return new Top10
                {
                    Symbol = symbol,
                    Name = name,
                    Close = (float)close,
                    PrevClose = (float)prevClose,
                    PercentChange = (float)percentChange
                };
            }

            logger.LogWarning("Gold entry was not found in top10 array.");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch stock summary from external API.");
            return null;
        }
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static double? GetDouble(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var value))
        {
            return null;
        }

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var numericValue))
        {
            return numericValue;
        }

        if (value.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        var raw = value.GetString();
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var invariantValue))
        {
            return invariantValue;
        }

        var normalized = raw.Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var normalizedValue)
            ? normalizedValue
            : null;
    }
}
