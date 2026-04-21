using NewsSite.Models.APIs;

namespace NewsSite.Services.Implementations
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;

        public WeatherService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<WeatherForecast> GetWeatherAsync()
        {
            var url = "http://weatherapi.dreammaker-it.se/forecast?City=Stockholm&Language=Eng";
            try
            {
                
                var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);
                if (weather != null) return weather;
            }
            catch {}

            return new WeatherForecast();
        }
    }
}

