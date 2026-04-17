namespace NewsSite.Models.ViewModels
{
    public class ArticleSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }
    }
}