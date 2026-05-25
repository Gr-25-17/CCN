using CCN.Jobs.Functions.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace CCN.Jobs.Functions.Functions;

public class WeatherFunction(
    ILogger<WeatherFunction> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration config)
{
    private static readonly string[] Cities = ["Stockholm", "Göteborg", "Malmö", "Kiruna", "Linköping"];

    [Function(nameof(WeatherFunction))]
    public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("Weather update triggered at: {Time}", DateTime.Now);

        var baseUrl = config["WEATHER_API_URL"] ?? throw new InvalidOperationException("Missing WEATHER_API_URL");
        var lang = config["WEATHER_API_LANG"] ?? "en";
        var connectionString = config["AzureWebJobsStorage"] ?? throw new InvalidOperationException("Missing Storage ConnectionString");

        var tableClient = new TableClient(connectionString, "WeatherData");
        await tableClient.CreateIfNotExistsAsync();

        using var client = httpClientFactory.CreateClient();

        foreach (var city in Cities)
        {
            var url = $"{baseUrl}?city={city}&lang={lang}";
            var weather = await client.GetFromJsonAsync<WeatherForecast>(url);

            if (weather is not { })
            {
                logger.LogWarning("No weather data returned for {City}", city);
                continue;
            }

            var entity = new WeatherEntity
            {
                PartitionKey = city,
                RowKey = (DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks).ToString("d19"),
                City = weather.City ?? city,
                TemperatureC = weather.TemperatureC,
                Humidity = weather.Humidity,
                WindSpeed = weather.WindSpeed,
                IconUrl = weather.Icon?.Url ?? string.Empty,
                IconCode = weather.Icon?.Code ?? string.Empty,
                DateString = weather.Date.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await tableClient.UpsertEntityAsync(entity);
            logger.LogInformation("Successfully saved weather for {City}", city);
        }
    }
}