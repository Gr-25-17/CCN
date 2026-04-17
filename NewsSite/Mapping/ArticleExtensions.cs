using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class ArticleExtensions
    {
        public static ArticleViewModel MapToArticleViewModel(this Article a)
        {
            return new ArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name,
                AuthorName = !string.IsNullOrWhiteSpace(a.AuthorName)
                    ? a.AuthorName
                    : (a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Okänd skribent"),
                IsReadyForPublish = a.IsReadyForPublish,
                ViewsCount = a.ViewsCount,
                LikesCount = a.LikesCount,
                CreatedAt = a.CreatedAt,
                Summary = a.Summary,
                Content = a.Content,
                Slug = a.Slug,
                ImageUrl = a.ImageUrl,
                MetaTitle = a.MetaTitle,
                MetaDescription = a.MetaDescription,
                IsEditorsChoice = a.IsEditorsChoice,
                IsPremium = a.IsPremium
            };
        }

        public static ArticleSummaryViewModel MapToSummaryViewModel(this Article a)
        {
            return new ArticleSummaryViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Summary = a.Summary,
                Slug = a.Slug,
                ImageUrl = a.ImageUrl,
                CreatedAt = a.CreatedAt,
                CategoryName = a.Category?.Name ?? string.Empty,
                AuthorName = !string.IsNullOrWhiteSpace(a.AuthorName)
                    ? a.AuthorName
                    : (a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Okänd skribent"),
                IsPremium = a.IsPremium,
                ViewsCount = a.ViewsCount,
                LikesCount = a.LikesCount
            };
        }
        public static Article ToArticleEntity(this ArticleViewModel model, string authorId, string authorName, string sanitizedContent, string slug)
        {
            return new Article
            {
                Title = model.Title,
                Summary = model.Summary,
                Content = sanitizedContent,
                Slug = slug,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                AuthorId = authorId,
                AuthorName = authorName,
                IsReadyForPublish = model.IsReadyForPublish,
                IsEditorsChoice = model.IsEditorsChoice,
                IsPremium = model.IsPremium,
                CreatedAt = DateTime.Now,
                MetaTitle = model.MetaTitle ?? model.Title,
                MetaDescription = model.MetaDescription ?? model.Summary
            };
        }

        public static void UpdateArticleEntity(this ArticleViewModel model, Article existingArticle, string sanitizedContent)
        {
            existingArticle.Title = model.Title;
            existingArticle.Summary = model.Summary;
            existingArticle.Content = sanitizedContent;
            existingArticle.ImageUrl = model.ImageUrl ?? existingArticle.ImageUrl;
            existingArticle.IsReadyForPublish = model.IsReadyForPublish;
            existingArticle.IsEditorsChoice = model.IsEditorsChoice;
            existingArticle.IsPremium = model.IsPremium;
            existingArticle.CategoryId = model.CategoryId;
            existingArticle.MetaTitle = model.MetaTitle ?? model.Title;
            existingArticle.MetaDescription = model.MetaDescription ?? model.Summary;
        }
    }
}