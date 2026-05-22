using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

public class GoldPrice : ITableEntity
{
    //Azure Tables
    public string PartitionKey { get; set; } = "Gold";
    public string? RowKey { get; set; } 
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }


    public double Close { get; set; }
    public double PrevClose { get; set; }
    public double PercentChange { get; set; }

    public string? Name { get; set; }
    public string? Symbol { get; set; }
}