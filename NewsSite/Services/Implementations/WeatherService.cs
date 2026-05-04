using NewsSite.Models.APIs;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherForecast> GetWeatherAsync()
        {
            var url = "http://localhost:7192/api/GetWeather?city=Stockholm";

            try
            {
                var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);
                if (weather != null) return weather;
            }
            catch { }

            return new WeatherForecast();
        }
    }
}

