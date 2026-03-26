using NewsSite.Models.Entities;

namespace NewsSite.Models.Entities
{
    public class ArticleLike
    {
        public int Id { get; set; }
        public int ArticleId { get; set; }
        public Article? Article { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}