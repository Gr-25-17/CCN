using API_Weather.Models;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

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

        var city = Environment.GetEnvironmentVariable("WEATHER_API_CITY") ?? "Stockholm";
        var language = Environment.GetEnvironmentVariable("WEATHER_API_LANG") ?? "Eng";

        var url = $"{baseUrl}?City={city}&Language={language}";

        try
        {
            using var client = httpClientFactory.CreateClient();
            var weather = await client.GetFromJsonAsync<WeatherForecast>(url);

            // Modern pattern matching istället för != null
            if (weather is { })
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                    ?? throw new InvalidOperationException("Missing configuration: AzureWebJobsStorage");

                // Arkitekturell notis: Om BlobServiceClient återanvänds hårt på sikt bör den injiceras via DI i Program.cs
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient("weather-cache");
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient("current-weather.json");
                var jsonData = JsonSerializer.Serialize(weather);

                // Modern using-deklaration minimerar kod-nesting
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonData));
                await blobClient.UploadAsync(stream, overwrite: true);

                logger.LogInformation("Successfully updated weather for {City}", city);
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