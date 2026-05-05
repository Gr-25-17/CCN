using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API_Weather.Models
{
    public class WeatherForecast
    {
        
        public string CurrentWeather { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Lang { get; set; } = string.Empty;
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public DateTime Date { get; set; }
        public int UnixTime { get; set; }
        public Icon? Icon { get; set; }
    }

    public class Icon
    {
        public string Url { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}
