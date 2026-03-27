using NewsSite.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        [NotMapped]
        public int LikesCount => Likes?.Count ?? 0;
        public int ViewsCount { get; set; }

        public bool IsArchived { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsEditorsChoice { get; set; }
        public bool IsReadyForPublish { get; set; }
        public bool IsLocked { get; set; }
        public bool IsPremium { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? AuthorId { get; set; }
        public ApplicationUser? Author { get; set; }

        public ICollection<ArticleLike> Likes { get; set; } = new List<ArticleLike>();
    }
}