namespace NewsSite.Models.ViewModels
{
    public class WeatherViewModel
    {
        public string City { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public string IconUrl { get; set; } = string.Empty;
        public string IconCode { get; set; } = string.Empty;
    }
}
