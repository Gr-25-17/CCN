using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using Ganss.Xss;
using System.Text.RegularExpressions;

namespace NewsSite.Services.Implementations
{
    public class ArticleService(
        IArticleRepository articleRepository,
        IUserRepository userRepository) : IArticleService
    {
        public async Task<IEnumerable<Article>> GetLatestAsync(int count) => await articleRepository.GetLatestAsync(count);
        public async Task<IEnumerable<Article>> GetMostPopularAsync(int count) => await articleRepository.GetMostPopularAsync(count);
        public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count) => await articleRepository.GetEditorChoiceAsync(count);
        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize) => await articleRepository.GetByCategoryAsync(categoryId, page, pageSize);
        public async Task<Article?> GetBySlugAsync(string slug) => await articleRepository.GetBySlugAsync(slug);
        public async Task IncrementViewCountAsync(int articleId) => await articleRepository.IncrementViewCountAsync(articleId);

        public async Task<IEnumerable<ArticleViewModel>> GetBackendArticlesAsync(string userId, bool canSeeAll)
        {
            var articles = canSeeAll
                ? await articleRepository.GetAllBackendArticlesAsync()
                : await articleRepository.GetByAuthorAsync(userId);

            return articles.Select(a => new ArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                IsReadyForPublish = a.IsReadyForPublish,
                CreatedAt = a.CreatedAt,
                CategoryName = a.Category?.Name ?? string.Empty,

                // UPPDATERAD LOGIK: 
                // 1. Kolla om vi har ett sparat namn (från nya systemet)
                // 2. Fallback till att bygga namnet från Author-objektet (för gamla artiklar)
                // 3. Annars "Okänd"
                AuthorName = !string.IsNullOrWhiteSpace(a.AuthorName)
                    ? a.AuthorName
                    : (a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : "Okänd skribent"),

                ViewsCount = a.ViewsCount,
                LikesCount = a.Likes?.Count ?? 0
            });
        }

        public async Task<ArticleViewModel> GetEditorModelAsync()
        {
            var categories = await articleRepository.GetAllCategoriesAsync();
            return new ArticleViewModel
            {
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
        }

        public async Task CreateAsync(ArticleViewModel model, string authorId)
        {
            var user = await userRepository.GetUserByIdAsync(authorId);
            var sanitizer = new HtmlSanitizer();

            var article = new Article
            {
                Title = model.Title,
                Summary = model.Summary,
                Content = sanitizer.Sanitize(model.Content),
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                AuthorId = authorId,
                AuthorName = user != null ? $"{user.FirstName} {user.LastName}" : "Okänd författare",
                IsReadyForPublish = model.IsReadyForPublish,
                IsEditorsChoice = model.IsEditorsChoice,
                IsPremium = model.IsPremium,
                MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? model.Title : model.MetaTitle,
                MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? model.Summary : model.MetaDescription,
                Slug = await GenerateUniqueSlugAsync(model.Title),
                CreatedAt = DateTime.UtcNow
            };

            await articleRepository.AddAsync(article);
        }

        public async Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll)
        {
            var article = await articleRepository.GetByIdAsync(model.Id);
            if (article == null || article.IsLocked || (!canSeeAll && article.AuthorId != userId)) return false;

            if (article.Title != model.Title)
            {
                article.Slug = await GenerateUniqueSlugAsync(model.Title);
            }

            var sanitizer = new HtmlSanitizer();
            article.Title = model.Title;
            article.Summary = model.Summary;
            article.Content = sanitizer.Sanitize(model.Content);
            article.ImageUrl = model.ImageUrl;
            article.CategoryId = model.CategoryId;
            article.IsReadyForPublish = model.IsReadyForPublish;
            article.IsEditorsChoice = model.IsEditorsChoice;
            article.IsPremium = model.IsPremium;
            article.MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? model.Title : model.MetaTitle;
            article.MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? model.Summary : model.MetaDescription;

            await articleRepository.UpdateAsync(article);
            return true;
        }

        public async Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId)
        {
            var isLiked = await articleRepository.HasUserLikedArticleAsync(articleId, userId);

            if (isLiked) await articleRepository.RemoveLikeAsync(articleId, userId);
            else await articleRepository.AddLikeAsync(articleId, userId);

            var count = await articleRepository.GetLikesCountAsync(articleId);
            return (!isLiked, count);
        }

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId) => await articleRepository.HasUserLikedArticleAsync(articleId, userId);

        public async Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll)
        {
            var article = await articleRepository.GetByIdAsync(id);
            if (article == null || (!canSeeAll && article.AuthorId != userId)) return null;

            var categories = await articleRepository.GetAllCategoriesAsync();
            return new ArticleViewModel
            {
                Id = article.Id,
                Title = article.Title,
                Summary = article.Summary,
                Content = article.Content,
                ImageUrl = article.ImageUrl,
                CategoryId = article.CategoryId,
                IsReadyForPublish = article.IsReadyForPublish,
                IsEditorsChoice = article.IsEditorsChoice,
                IsPremium = article.IsPremium,
                MetaTitle = article.MetaTitle,
                MetaDescription = article.MetaDescription,
                Slug = article.Slug,
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
        }

        public async Task<string> GenerateUniqueSlugAsync(string title)
        {
            var slug = title.ToLower()
                .Replace("å", "a").Replace("ä", "a").Replace("ö", "o");

            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-").Trim('-');

            if (slug.Length > 45) slug = slug.Substring(0, 45).Trim('-');

            var finalSlug = slug;
            var counter = 1;

            while (await articleRepository.SlugExistsAsync(finalSlug))
            {
                finalSlug = $"{slug}-{counter}";
                counter++;
            }

            return finalSlug;
        }
    }
}