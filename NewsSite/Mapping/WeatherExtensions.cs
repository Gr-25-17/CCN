using NewsSite.Models.APIs;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class WeatherExtensions
    {
        public static WeatherBasicVM ToViewModel(this WeatherForecast response)
        {
            return new WeatherBasicVM
            {
                UrlIcon = response.Icon?.Url ?? string.Empty,
                TemperatureC = response.TemperatureC,
                TemperatureDisplay = $"{response.TemperatureC:F1}°C"
            };
        }

        public static WeatherForecast ToEntity(this WeatherBasicVM vm)
        {
            return new WeatherForecast
            {
                TemperatureC = vm.TemperatureC,
                Icon = new Icon
                {
                    Url = vm.UrlIcon
                }
            };
        }

        public static WeatherViewModel ToWeatherViewModel(this WeatherForecast response)
        {
            return new WeatherViewModel
            {
                City = response.City ?? string.Empty,
                TemperatureC = response.TemperatureC,
                Humidity = response.Humidity,
                WindSpeed = response.WindSpeed,
                Date = response.Date,
                IconUrl = response.Icon?.Url ?? string.Empty,
                IconCode = response.Icon?.Code ?? string.Empty
            };
        }
    }
}