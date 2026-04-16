using System.Text.Json.Serialization;


    public class StockPrice
    {
        [JsonPropertyName("top10")]
        public Top10[] Top10Stock { get; set; }
    }


    public class Top10
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
        public float Close { get; set; }
        public float PrevClose { get; set; }
        public float PercentChange { get; set; }
    }


