using System.Text.Json.Serialization;

namespace CCN.Jobs.Functions.Models;

public class WeatherForecast
{
    [JsonPropertyName("summary")]
    public string CurrentWeather { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; set; } = string.Empty;

    [JsonPropertyName("temperatureC")]
    public int TemperatureC { get; set; }

    [JsonPropertyName("temperatureF")]
    public int TemperatureF { get; set; }

    [JsonPropertyName("humidity")]
    public int Humidity { get; set; }

    [JsonPropertyName("windSpeed")]
    public double WindSpeed { get; set; }

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("unixTime")]
    public int UnixTime { get; set; }

    [JsonPropertyName("icon")]
    public Icon? Icon { get; set; }
}

public class Icon
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}
