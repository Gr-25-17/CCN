using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
// C# 12 Primary Constructor med injicerad ILogger för att undvika tysta krascher
public class WeatherService(
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    ILogger<WeatherService> logger) : IWeatherService
{
    public async Task<WeatherForecast?> GetWeatherAsync()
    {
        try
        {
            var connectionString = config.GetConnectionString("AzureWebJobsStorage")
                ?? throw new InvalidOperationException("Missing AzureWebJobsStorage connection string");

            // Centraliserad config. Fallback till Stockholm för säkerhet.
            var city = config["WeatherSettings:City"] ?? "Stockholm";
            var tableClient = new TableClient(connectionString, "WeatherData");
        {
            // Nu matchar vi Azure-funktionens dynamiska PartitionKey
            var latestEntity = await tableClient.QueryAsync<TableEntity>(
                filter: $"PartitionKey eq '{city}'",
                maxPerPage: 1)
                .FirstOrDefaultAsync();

            if (latestEntity is null)
            {
                logger.LogWarning("Ingen väderdata hittades i Table Storage för staden {City}", city);
                return null;
            }
                {
            return new WeatherForecast
            {
                City = latestEntity.GetString("City") ?? "Okänd",
                TemperatureC = latestEntity.GetDouble("TemperatureC") ?? 0,
                Humidity = latestEntity.GetDouble("Humidity") ?? 0,
                WindSpeed = latestEntity.GetDouble("WindSpeed") ?? 0,
                Date = DateTime.TryParse(latestEntity.GetString("DateString"), out var d) ? d : DateTime.UtcNow,
                Icon = new Icon
                {
                    Url = latestEntity.GetString("IconUrl") ?? string.Empty,
                    Code = latestEntity.GetString("IconCode") ?? string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            // Fånga och logga undantaget explicit istället för att tyst returnera null
            logger.LogError(ex, "Ett kritiskt fel uppstod vid hämtning av väderdata från Table Storage.");
            return null;
                return new WeatherForecast();
            }
            catch
            {
                return new WeatherForecast();
            }
        }
    }
}