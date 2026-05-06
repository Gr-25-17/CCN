using API_Weather.Models;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace API_Weather
{
    public class WeatherFunction
    {
        private readonly ILogger<WeatherFunction> _logger;
        private readonly HttpClient _httpClient;

        public WeatherFunction(ILogger<WeatherFunction> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        [Function("UpdateWeather")]
        public async Task Run([TimerTrigger("0 0 */6 * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Weather update triggered at: {DateTime.Now}");

            string city = "Stockholm";
            string language = "Eng";
            var url = $"http://weatherapi.dreammaker-it.se/forecast?City={city}&Language={language}";

            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating weather for {city}");
            }
        }
    }
}