using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels
{
    public class ArticleViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        [Required]
        public string Summary { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? ImageExternalUrl { get; set; }
        public bool IsReadyForPublish { get; set; }
        public bool IsEditorsChoice { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CategoryName { get; set; }
        public string? AuthorName { get; set; }

        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public List<SelectListItem> Categories { get; set; } = new();
    }
}