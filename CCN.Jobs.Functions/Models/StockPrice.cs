using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CCN.Jobs.Functions.Models;

public class StockPrice
{
    [JsonPropertyName("top10")]
    public Top10[] Top10Stock { get; set; } = [];
}

public class Top10
{
    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Symbol { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float Close { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float PrevClose { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public float PercentChange { get; set; }
}
