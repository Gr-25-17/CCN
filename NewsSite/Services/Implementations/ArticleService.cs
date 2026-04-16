using Ganss.Xss;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using System.Text.RegularExpressions;

namespace NewsSite.Services.Implementations
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IUserRepository _userRepository;

        public ArticleService(IArticleRepository articleRepository, IUserRepository userRepository)
        {
            _articleRepository = articleRepository;
            _userRepository = userRepository;
        }

        private static ArticleViewModel MapToArticleViewModel(Article a)
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
        private static ArticleSummaryViewModel MapToSummaryViewModel(Article a)
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

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetLatestAsync(int count)
        {
            var articles = await _articleRepository.GetLatestAsync(count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularAsync(int count)
        {
            var articles = await _articleRepository.GetMostPopularAsync(count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceAsync(int count)
        {
            var articles = await _articleRepository.GetEditorChoiceAsync(count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var articles = await _articleRepository.GetByCategoryAsync(categoryId, page, pageSize);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            var articles = await _articleRepository.GetLatestByCategoryIdsAsync(categoryIds, count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            var articles = await _articleRepository.GetMostPopularByCategoryIdsAsync(categoryIds, count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            var articles = await _articleRepository.GetEditorChoiceByCategoryIdsAsync(categoryIds, count);
            return articles.Select(MapToSummaryViewModel);
        }

        public async Task<IEnumerable<ArticleViewModel>> GetBackendArticlesAsync(string userId, bool canSeeAll)
        {
            var articles = canSeeAll
                ? await _articleRepository.GetAllBackendArticlesAsync()
                : await _articleRepository.GetByAuthorAsync(userId);

            return articles.Select(MapToArticleViewModel);
        }

        public async Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null) return null;

            if (!canSeeAll && article.AuthorId != userId) return null;

            var categories = await _articleRepository.GetAllCategoriesAsync();

            var vm = MapToArticleViewModel(article);
            vm.Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            return vm;
        }

        public async Task CreateAsync(ArticleViewModel model, string authorId)
        {
            var sanitizer = new HtmlSanitizer();
            var user = await _userRepository.GetUserByIdAsync(authorId);

            var authorName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Okänd Skribent";

            var article = new Article
            {
                Title = model.Title,
                Summary = model.Summary,
                Content = sanitizer.Sanitize(model.Content),
                Slug = string.IsNullOrWhiteSpace(model.Slug) ? await GenerateUniqueSlugAsync(model.Title) : model.Slug,
                ImageUrl = model.ImageUrl,
                MetaTitle = model.MetaTitle ?? model.Title,
                MetaDescription = model.MetaDescription ?? model.Summary,
                IsReadyForPublish = model.IsReadyForPublish,
                IsEditorsChoice = model.IsEditorsChoice,
                IsPremium = model.IsPremium,
                CategoryId = model.CategoryId,
                AuthorId = authorId,
                AuthorName = authorName,
                CreatedAt = DateTime.UtcNow
            };

            await _articleRepository.AddAsync(article);
        }

        public async Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll)
        {
            var article = await _articleRepository.GetByIdAsync(model.Id);
            if (article == null) return false;

            if (!canSeeAll && article.AuthorId != userId) return false;

            var sanitizer = new HtmlSanitizer();

            article.Title = model.Title;
            article.Summary = model.Summary;
            article.Content = sanitizer.Sanitize(model.Content);
            article.ImageUrl = model.ImageUrl ?? article.ImageUrl;
            article.MetaTitle = model.MetaTitle ?? model.Title;
            article.MetaDescription = model.MetaDescription ?? model.Summary;
            article.IsReadyForPublish = model.IsReadyForPublish;
            article.IsEditorsChoice = model.IsEditorsChoice;
            article.IsPremium = model.IsPremium;
            article.CategoryId = model.CategoryId;

            if (string.IsNullOrWhiteSpace(article.Slug))
            {
                article.Slug = await GenerateUniqueSlugAsync(article.Title);
            }

            await _articleRepository.UpdateAsync(article);
            return true;
        }

        public async Task<ArticleViewModel> GetEditorModelAsync()
        {
            var categories = await _articleRepository.GetAllCategoriesAsync();
            return new ArticleViewModel
            {
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

            while (await _articleRepository.SlugExistsAsync(finalSlug))
            {
                finalSlug = $"{slug}-{counter}";
                counter++;
            }

            return finalSlug;
        }

        public async Task<ArticleViewModel?> GetBySlugAsync(string slug)
        {
            var article = await _articleRepository.GetBySlugAsync(slug);

            if (article == null)
            {
                return null;
            }

            return MapToArticleViewModel(article);
        }

        public async Task IncrementViewCountAsync(int articleId)
        {
            await _articleRepository.IncrementViewCountAsync(articleId);
        }

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId) => await _articleRepository.HasUserLikedArticleAsync(articleId, userId);
        

        public async Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId)
        {
            var hasLiked = await _articleRepository.HasUserLikedArticleAsync(articleId, userId);

            if (hasLiked)
            {
                await _articleRepository.RemoveLikeAsync(articleId, userId);
            }
            else
            {
                await _articleRepository.AddLikeAsync(articleId, userId);
            }

            var likesCount = await _articleRepository.GetLikesCountAsync(articleId);
            return (!hasLiked, likesCount);
        }

    }
}