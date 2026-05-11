using Azure.Data.Tables;
using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class WeatherService(
        IConfiguration config,
        ILogger<WeatherService> logger) : IWeatherService
    {
        public async Task<WeatherForecast?> GetWeatherAsync()
        {
            try
            {
                var connectionString = config.GetConnectionString("AzureWebJobsStorage");

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    logger.LogCritical("Saknar AzureWebJobsStorage i konfigurationen. Avbryter hämtning av väderdata.");
                    return null;
                }

                var city = config["WeatherSettings:City"] ?? "Stockholm";
                var tableClient = new TableClient(connectionString, "WeatherData");

                var latestEntity = await tableClient.QueryAsync<TableEntity>(
                    filter: $"PartitionKey eq '{city}'",
                    maxPerPage: 1)
                    .FirstOrDefaultAsync();

                if (latestEntity is not { })
                {
                    logger.LogWarning("Ingen väderdata hittades i Table Storage för staden {City}", city);
                    return null;
                }

                return new WeatherForecast
                {
                    City = latestEntity.GetString("City") ?? "Okänd",
                    TemperatureC = GetSafeDouble(latestEntity, "TemperatureC"),
                    Humidity = GetSafeDouble(latestEntity, "Humidity"),
                    WindSpeed = GetSafeDouble(latestEntity, "WindSpeed"),
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
                logger.LogError(ex, "Kritiskt fel vid hämtning av väderdata från Table Storage.");
                return null;
            }
        }


        private static double GetSafeDouble(TableEntity entity, string key)
        {
            if (!entity.TryGetValue(key, out var value) || value is null)
            {
                return 0;
            }

            return value switch
            {
                double d => d,
                int i => i, 
                long l => l,
                string s when double.TryParse(s, out var parsed) => parsed,
                _ => 0
            };
        }
    }
}