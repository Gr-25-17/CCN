using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class ArticleExtensions
    {
        private const string UnknownAuthor = "Okänd skribent";
        private const string PlaceholderImagePath = "/images/placeholder.jpg";
        private const string DefaultImageSize = "med";
        public static ArticleViewModel MapToArticleViewModel(this Article a)
        {
            return new ArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                CategoryId = a.CategoryId,
                CategoryName = a.Category?.Name,
                AuthorName = a.ResolveAuthorName(),
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
                AuthorName = a.ResolveAuthorName(),
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
            existingArticle.ImageUrl = model.ImageUrl;
            existingArticle.IsReadyForPublish = model.IsReadyForPublish;
            existingArticle.IsEditorsChoice = model.IsEditorsChoice;
            existingArticle.IsPremium = model.IsPremium;
            existingArticle.CategoryId = model.CategoryId;
            existingArticle.MetaTitle = model.MetaTitle ?? model.Title;
            existingArticle.MetaDescription = model.MetaDescription ?? model.Summary;
        }
        public static SearchArticleVM MapToSearchViewModel(this Article article)
        {
            return new SearchArticleVM
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                Slug = article.Slug,
                ImageUrl = article.ImageUrl,
                CategoryName = article.Category?.Name ?? "Allmänt",
                AuthorName = article.ResolveAuthorName(),
                CreatedAt = article.CreatedAt,
                IsPremium = article.IsPremium,
                IsArchived = article.IsArchived,
                ViewsCount = article.ViewsCount,
                LikesCount = article.LikesCount
            };
        }
        private static string ResolveAuthorName(this Article article)
        {
            if (!string.IsNullOrWhiteSpace(article.AuthorName))
            {
                return article.AuthorName;
            }

            return article.Author is null
                ? UnknownAuthor
                : $"{article.Author.FirstName} {article.Author.LastName}";
        }


        private static string NormalizeImageSize(string? size)
        {
            var normalizedSize = size?.Trim().ToLowerInvariant();

            return normalizedSize switch
            {
                "min" or "med" or "full" => normalizedSize,
                _ => DefaultImageSize
            };
        }

        public static string ResolveImageUrl(this string? imageUrl, string size, IConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return PlaceholderImagePath;
            if (imageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)) return imageUrl;

            var normalizedImageUrl = imageUrl.TrimStart('/');
            var normalizedSize = NormalizeImageSize(size);

            var baseUrl = config["StorageSettings:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                return $"/images/{normalizedImageUrl}";
            }

            var normalizedBaseUrl = baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : $"{baseUrl}/";
            var container = normalizedImageUrl.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                ? "articles-full"
                : $"articles-{normalizedSize}";

            return $"{normalizedBaseUrl}{container}/{normalizedImageUrl}";
        }
    }
}
