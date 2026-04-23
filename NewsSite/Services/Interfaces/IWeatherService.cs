using NewsSite.Models.APIs;

namespace NewsSite.Services.Interfaces
{
    public interface IWeatherService
    {
        Task<WeatherForecast> GetWeatherAsync();
    }
}
