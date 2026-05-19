using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Models
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public bool IsReadyForPublish { get; set; }
        public bool IsEditorsChoice { get; set; }
        public bool IsPremium { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CategoryName { get; set; }
        public string? AuthorName { get; set; }
        public int ViewsCount { get; set; }
        public int LikesCount { get; set; }
        public int CategoryId { get; set; }
    }
}
