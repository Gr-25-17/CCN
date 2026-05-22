using Azure;
using Azure.Data.Tables;

namespace CCN.Jobs.Functions.Models;

public class WeatherEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "Weather";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string City { get; set; } = string.Empty;
    public double TemperatureC { get; set; }
    public double Humidity { get; set; }
    public double WindSpeed { get; set; }
    public string IconUrl { get; set; } = string.Empty;
    public string IconCode { get; set; } = string.Empty;
    public string DateString { get; set; } = string.Empty;
}
