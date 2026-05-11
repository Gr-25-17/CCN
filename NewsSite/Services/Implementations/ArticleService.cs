using Ganss.Xss;
using NewsSite.Mapping;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using System.Net;
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
        private string SanitizeContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content)) return string.Empty;

            var sanitizer = new HtmlSanitizer();

            var decodedContent = WebUtility.HtmlDecode(content);

            var sanitized = sanitizer.Sanitize(decodedContent);
            sanitized = Regex.Replace(sanitized, @"<(p|div|span|strong|em)>\s*</\1>", string.Empty);

            return sanitized;
        }
        public async Task<IEnumerable<ArticleSummaryViewModel>> GetLatestAsync(int count)
            => (await _articleRepository.GetLatestAsync(count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularAsync(int count)
            => (await _articleRepository.GetMostPopularAsync(count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceAsync(int count)
            => (await _articleRepository.GetEditorChoiceAsync(count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetByCategoryAsync(int categoryId, int page, int pageSize)
            => (await _articleRepository.GetByCategoryAsync(categoryId, page, pageSize)).Select(a=>a.MapToSummaryViewModel());


        public async Task<IEnumerable<ArticleSummaryViewModel>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count)
            => (await _articleRepository.GetLatestByCategoryIdsAsync(categoryIds, count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularByCategoryIdsAsync(List<int> categoryIds, int count)
            => (await _articleRepository.GetMostPopularByCategoryIdsAsync(categoryIds, count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceByCategoryIdsAsync(List<int> categoryIds, int count)
            => (await _articleRepository.GetEditorChoiceByCategoryIdsAsync(categoryIds, count)).Select(a => a.MapToSummaryViewModel());
        

        public async Task<IEnumerable<ArticleViewModel>> GetBackendArticlesAsync(string userId, bool canSeeAll)
            => (canSeeAll
                ? await _articleRepository.GetAllBackendArticlesAsync()
                : await _articleRepository.GetByAuthorAsync(userId))
                .Select(a => a.MapToArticleViewModel());
        

        public async Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll)
        {
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null) return null;
            if (!canSeeAll && article.AuthorId != userId) return null;

            var viewModel = article.MapToArticleViewModel();
            var categories = await _articleRepository.GetAllCategoriesAsync();
            viewModel.Categories = categories.Select(c => c.ToCategorySelectListItem()).ToList();

            return viewModel;
        }
        public async Task CreateAsync(ArticleViewModel model, string authorId)
        {
            var sanitizedContent = SanitizeContent(model.Content);

            var user = await _userRepository.GetUserByIdAsync(authorId);
            var authorName = user != null ? $"{user.FirstName} {user.LastName}" : "Okänd";
            var slug = await GenerateUniqueSlugAsync(model.Title);

            var article = model.ToArticleEntity(authorId, authorName, sanitizedContent, slug);
            await _articleRepository.AddAsync(article);
        }

        public async Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll)
        {
            var existingArticle = await _articleRepository.GetByIdAsync(model.Id);
            if (existingArticle == null) return false;
            if (!canSeeAll && existingArticle.AuthorId != userId) return false;

            var sanitizedContent = SanitizeContent(model.Content);
            bool titleChanged = existingArticle.Title != model.Title;

            model.UpdateArticleEntity(existingArticle, sanitizedContent);

            if (titleChanged)
            {
                existingArticle.Slug = await GenerateUniqueSlugAsync(model.Title);
            }

            await _articleRepository.UpdateAsync(existingArticle);
            return true;
        }

        public async Task<ArticleViewModel> GetEditorModelAsync()
            => new ArticleViewModel
            {
                Categories = (await _articleRepository.GetAllCategoriesAsync())
                    .Select(c => c.ToCategorySelectListItem())
                    .ToList()
            };


        public async Task<string> GenerateUniqueSlugAsync(string title)
        {
            var slug = title.ToLower().Trim();

            slug = slug.Replace("å", "a")
                       .Replace("ä", "a")
                       .Replace("ö", "o");

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
            => (await _articleRepository.GetBySlugAsync(slug))?.MapToArticleViewModel();

        public async Task IncrementViewCountAsync(int articleId)
            =>  await _articleRepository.IncrementViewCountAsync(articleId);
        

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId)
            => await _articleRepository.HasUserLikedArticleAsync(articleId, userId);
        

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

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetAllArticlesSortedByPreferencesAsync(List<int> preferredCategoryIds, List<string> preferredAuthorIds, int count)
    => await _articleRepository.GetAllArticlesSortedByPreferencesAsync(preferredCategoryIds, preferredAuthorIds, count);

    }
}