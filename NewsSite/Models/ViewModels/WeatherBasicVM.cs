namespace NewsSite.Models.ViewModels
{
    public class WeatherBasicVM
    {

        public string? UrlIcon { get; set; }

        public double TemperatureC { get; set; }

        public string TemperatureDisplay => $"{TemperatureC} °C";


    }
}
