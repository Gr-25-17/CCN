namespace NewsSite.Models.ViewModels
{
    public class AuthorViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public int ArticleCount { get; set; }
    }
}
