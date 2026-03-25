using NewsSite.Models.APIs;

namespace NewsSite.Models.ViewModels
{
    public class WeatherBasicVM
    {

        public string UrlIcon { get; set; }

        public int TemperatureC { get; set; }

        public string TemperatureDisplay => $"{TemperatureC} °C";


    }
}
