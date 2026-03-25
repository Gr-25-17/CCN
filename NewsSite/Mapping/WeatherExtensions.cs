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

    }
}