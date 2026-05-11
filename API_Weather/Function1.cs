using API_Weather.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace API_Weather;


public class WeatherFunction(
    ILogger<WeatherFunction> logger,
    IHttpClientFactory httpClientFactory,
    IConfiguration config)
{
    [Function(nameof(WeatherFunction))]
    public async Task Run([TimerTrigger("0 */15 * * * *")] TimerInfo myTimer)
    {
        logger.LogInformation("Weather update triggered at: {Time}", DateTime.Now);

        var baseUrl = config["WEATHER_API_URL"] ?? throw new InvalidOperationException("Missing WEATHER_API_URL");
        var city = config["WEATHER_API_CITY"] ?? "Stockholm";
        var lang = config["WEATHER_API_LANG"] ?? "en";
        var connectionString = config["AzureWebJobsStorage"] ?? throw new InvalidOperationException("Missing Storage ConnectionString");

        var url = $"{baseUrl}?city={city}&lang={lang}";

        try
        {
            using var client = httpClientFactory.CreateClient();
            var weather = await client.GetFromJsonAsync<WeatherForecast>(url);

           
            if (weather is not { })
            {
                logger.LogWarning("No weather data returned for {City}", city);
                return;
            }

            var tableClient = new TableClient(connectionString, "WeatherData");
            await tableClient.CreateIfNotExistsAsync();

            

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

            logger.LogInformation("Successfully saved weather for {City} to Table Storage", city);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating weather for {City}", city);
        }
    }
}