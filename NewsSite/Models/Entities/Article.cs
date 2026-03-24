using NewsSite.Models.Entities;
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

        // Status-flaggor
        public bool IsArchived { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        public bool IsEditorsChoice { get; set; } = false;
        public bool IsReadyForPublish { get; set; } = false;
        public bool IsLocked { get; set; } = false;

        // Foreign Keys
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? AuthorId { get; set; }
        public ApplicationUser? Author { get; set; }
    }
}