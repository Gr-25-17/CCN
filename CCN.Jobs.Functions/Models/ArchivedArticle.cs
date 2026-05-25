using Azure;
using Azure.Data.Tables;

namespace CCN.Jobs.Functions.Models;

public class ArchivedArticle : ITableEntity
{
    public string PartitionKey { get; set; } = "ArchivedArticles";
    public string RowKey { get; set; } = string.Empty;
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ArchivedAt { get; set; }
}