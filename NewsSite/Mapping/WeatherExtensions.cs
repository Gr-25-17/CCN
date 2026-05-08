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
                UrlIcon = response.Icon?.Url,
                TemperatureC = response.TemperatureC
            };
        }

        
        public static WeatherViewModel ToWeatherViewModel(this WeatherForecast? response) => response switch
        {
            null => new WeatherViewModel(), // Eller hantera som null i vyn
            _ => new WeatherViewModel
            {
                City = response.City ?? "Okänd",
                TemperatureC = response.TemperatureC,
                IconUrl = response.Icon?.Url ?? "/img/default-weather.png", // Fallback-bild
                Date = response.Date == default ? DateTime.Now : response.Date
            }
        };

        public static WeatherForecast ToEntity(this WeatherBasicVM vm)
        {
            return new WeatherForecast
            {
                TemperatureC = vm.TemperatureC,
                Icon = new Icon
                {
                    Url = vm.UrlIcon ?? string.Empty
                }
            };
        }

    }
}