using Newtonsoft.Json;

namespace NewsSite.Models.APIs
{
    public class WeatherForecast
    {
        [JsonProperty("summary")]
        public string? CurrentWeather { get; set; } //Called "summary" before
        public string? City { get; set; }
        public string? Lang { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public int UnixTime { get; set; }
        public Icon Icon { get; set; } = new Icon { Url = "", Code = "" };
}

    public class Icon
    {
        public string? Url { get; set; }
        public string? Code { get; set; }
    }

}




