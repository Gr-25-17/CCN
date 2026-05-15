using Newtonsoft.Json;

namespace NewsSite.Models.APIs
{
    public class WeatherForecast
    {
        public string City { get; set; } = string.Empty;
        public double TemperatureC { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public Icon? Icon { get; set; }
    }

    public class Icon
    {
        public string Url { get; set; }
        public string Code { get; set; }
    }

}




