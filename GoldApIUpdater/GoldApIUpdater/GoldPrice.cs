using Azure;
using Azure.Data.Tables;

public class GoldPrice : ITableEntity
{
    // Mandatory for Azure Tables
    public string PartitionKey { get; set; } = "Gold";
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; } = ETag.All;

    // Your custom data from the API
    public float Close { get; set; }
    public float PrevClose { get; set; }
    public float PercentChange { get; set; }
}