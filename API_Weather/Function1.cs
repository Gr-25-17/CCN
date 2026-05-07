using API_Weather.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace API_Weather;

// C# 12 Primary Constructor för renare Dependency Injection
public class WeatherFunction(ILogger<WeatherFunction> logger, IHttpClientFactory httpClientFactory)
{
    [Function(nameof(WeatherFunction))]
    public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("Weather update triggered at: {Time}", DateTime.Now);

        // Guard clauses mot config. Fallbacks implementerade där rimligt.
        var baseUrl = Environment.GetEnvironmentVariable("WEATHER_API_URL")
            ?? throw new InvalidOperationException("Missing configuration: WEATHER_API_URL");

        [Function("UpdateWeather")]
        public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Weather update triggered at: {DateTime.Now}");

        try
        {
            using var client = httpClientFactory.CreateClient();
            var weather = await client.GetFromJsonAsync<WeatherForecast>(url);

            // Modern pattern matching istället för != null
            if (weather is { })
            {
                var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

                if (weather != null)
                {
                    var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                    var tableClient = new TableClient(connectionString, "WeatherData");
                    await tableClient.CreateIfNotExistsAsync();

                    var entity = new WeatherEntity
                    {
                        RowKey = DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
                        City = weather.City ?? "Stockholm",
                        TemperatureC = weather.TemperatureC,
                        Humidity = weather.Humidity,
                        WindSpeed = weather.WindSpeed,
                        IconUrl = weather.Icon?.Url ?? string.Empty,
                        IconCode = weather.Icon?.Code ?? string.Empty,
                        DateString = weather.Date.ToString("yyyy-MM-dd HH:mm:ss")
                    };

                    await tableClient.UpsertEntityAsync(entity);

                    _logger.LogInformation($"Successfully saved weather for {city} to Table Storage");
                }
                else
                {
                    _logger.LogWarning($"No weather data returned for {city}");
                }
            }
            else
            {
                logger.LogWarning("No weather data returned for {City}", city);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating weather for {City}", city);
        }
    }
}