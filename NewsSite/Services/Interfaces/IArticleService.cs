using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<ArticleSummaryViewModel>> GetLatestAsync(int count);
        Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularAsync(int count);
        Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceAsync(int count);
        Task<IEnumerable<ArticleSummaryViewModel>> GetByCategoryAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<ArticleViewModel>> GetBackendArticlesAsync(string userId, bool canSeeAll);
        Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll);
        Task CreateAsync(ArticleViewModel model, string authorId);
        Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll);
        Task<ArticleViewModel> GetEditorModelAsync();
        Task<string> GenerateUniqueSlugAsync(string title);
        Task<ArticleViewModel?> GetBySlugAsync(string slug);
        Task IncrementViewCountAsync(int articleId);
        Task<bool> HasUserLikedArticleAsync(int articleId, string userId);
        Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId);
        Task<IEnumerable<ArticleSummaryViewModel>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count);
        Task<IEnumerable<ArticleSummaryViewModel>> GetMostPopularByCategoryIdsAsync(List<int> categoryIds, int count);
        Task<IEnumerable<ArticleSummaryViewModel>> GetEditorChoiceByCategoryIdsAsync(List<int> categoryIds, int count);
    }
}