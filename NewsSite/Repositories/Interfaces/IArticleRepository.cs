using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task<IEnumerable<Article>> GetLatestAsync(int count);
        Task<IEnumerable<Article>> GetMostPopularAsync(int count);
        Task<IEnumerable<Article>> GetEditorChoiceAsync(int count);
        Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<Article>> GetByAuthorAsync(string authorId);
        Task<IEnumerable<Article>> GetAllBackendArticlesAsync();
        Task<Article?> GetByIdAsync(int id);
        Task AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<Article?> GetBySlugAsync(string slug);
        Task IncrementViewCountAsync(int articleId);
        Task<bool> HasUserLikedArticleAsync(int articleId, string userId);
        Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId);
    }
}