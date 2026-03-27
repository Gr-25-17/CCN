using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using Ganss.Xss;
using System.Text.RegularExpressions;

namespace NewsSite.Services.Implementations
{
    public class ArticleService(IArticleRepository articleRepository) : IArticleService
    {
        public async Task<IEnumerable<Article>> GetLatestAsync(int count) => await articleRepository.GetLatestAsync(count);
        public async Task<IEnumerable<Article>> GetMostPopularAsync(int count) => await articleRepository.GetMostPopularAsync(count);
        public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count) => await articleRepository.GetEditorChoiceAsync(count);
        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize) => await articleRepository.GetByCategoryAsync(categoryId, page, pageSize);

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
                AuthorName = a.Author != null ? $"{a.Author.FirstName} {a.Author.LastName}" : string.Empty,
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
            var sanitizer = new HtmlSanitizer();
            var sanitizedContent = sanitizer.Sanitize(model.Content);
            var generatedSlug = GenerateSlug(model.Title);

            var article = new Article
            {
                Title = model.Title,
                Summary = model.Summary,
                Content = sanitizedContent,
                ImageUrl = model.ImageUrl,
                CategoryId = model.CategoryId,
                AuthorId = authorId,
                IsReadyForPublish = model.IsReadyForPublish,
                IsEditorsChoice = model.IsEditorsChoice,
                MetaTitle = string.IsNullOrWhiteSpace(model.MetaTitle) ? model.Title : model.MetaTitle,
                MetaDescription = string.IsNullOrWhiteSpace(model.MetaDescription) ? model.Summary : model.MetaDescription,
                Slug = generatedSlug,
                IsPremium = model.IsPremium,
                ViewsCount = 0
            };

            await articleRepository.AddAsync(article);
        }

        private string GenerateSlug(string phrase)
        {
            string str = phrase.ToLower();
            str = str.Replace("å", "a").Replace("ä", "a").Replace("ö", "o");
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }

        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await articleRepository.GetBySlugAsync(slug);
        }
        public async Task IncrementViewCountAsync(int articleId)
        {
            await articleRepository.IncrementViewCountAsync(articleId);
        }

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId)
        {
            return await articleRepository.HasUserLikedArticleAsync(articleId, userId);
        }

        public async Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId)
        {
            return await articleRepository.ToggleLikeAsync(articleId, userId);
        }
        public async Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll)
        {
            var article = await articleRepository.GetByIdAsync(id);
            if (article == null || (!canSeeAll && article.AuthorId != userId))
            {
                return null;
            }

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

        public async Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll)
        {
            var article = await articleRepository.GetByIdAsync(model.Id);

            if (article == null || article.IsLocked || (!canSeeAll && article.AuthorId != userId))
            {
                return false;
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
            article.Slug = GenerateSlug(model.Title);

            await articleRepository.UpdateAsync(article);
            return true;
        }
    }
}