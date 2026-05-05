namespace NewsSite.Models.ViewModels
{
    public class SearchArticleVM
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsPremium { get; set; }
        public bool IsArchived { get; set; }
        public int ViewsCount { get; set; }
    }
}
