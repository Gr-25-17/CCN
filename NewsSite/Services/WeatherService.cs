using Microsoft.Extensions.Hosting;
using NewsSite.Models.APIs;
using NewsSite.Models.ViewModels;
using static System.Net.WebRequestMethods;

namespace NewsSite.Services
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
        
            var basicUrl = "http://weatherapi.dreammaker-it.se/forecast?";
            var url = $"{basicUrl}City=Stockholm&Language=Eng"; //later add loaction from user to add to the basicurl.
            
            var weather = await _httpClient.GetFromJsonAsync<WeatherForecast>(url);

            return weather ?? new WeatherForecast();
        }
    }
}
