using API_Weather.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [Function("GetWeather")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("GetWeather function triggered");



            string city = req.Query["city"].ToString() ?? "Stockholm";
            string language = req.Query["language"].ToString() ?? "Eng";

            var url = $"http://weatherapi.dreammaker-it.se/forecast?City={city}&Language={language}";

            try
            {
                var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

                if (weather != null)
                {
                    _logger.LogInformation($"Successfully fetched weather for {city}");
                    return new OkObjectResult(weather);
                }

                _logger.LogWarning($"No weather data returned for {city}");
                return new NotFoundObjectResult("Weather data not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching weather for {city}");
                return new StatusCodeResult(500);
            }
        }
    }
}