using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces
{
    public interface IArticleService
    {
        Task<IEnumerable<Article>> GetLatestAsync(int count);
        Task<IEnumerable<Article>> GetMostPopularAsync(int count);
        Task<IEnumerable<Article>> GetEditorChoiceAsync(int count);
        Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize);

        Task<IEnumerable<ArticleViewModel>> GetBackendArticlesAsync(string userId, bool canSeeAll);
        Task<ArticleViewModel?> GetForEditAsync(int id, string userId, bool canSeeAll);
        Task CreateAsync(ArticleViewModel model, string authorId);
        Task<bool> UpdateAsync(ArticleViewModel model, string userId, bool canSeeAll);
        Task<ArticleViewModel> GetEditorModelAsync();

        Task<Article?> GetBySlugAsync(string slug);
        Task IncrementViewCountAsync(int articleId);
        Task<bool> HasUserLikedArticleAsync(int articleId, string userId);
        Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId);
    }
}