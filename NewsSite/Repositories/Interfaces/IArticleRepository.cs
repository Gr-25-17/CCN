using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces
{
    public interface IArticleRepository
    {
        Task<Article?> GetBySlugAsync(string slug);
        Task<IEnumerable<Article>> GetLatestAsync(int count);
        Task<IEnumerable<Article>> GetMostPopularAsync(int count);
        Task<IEnumerable<Article>> GetEditorChoiceAsync(int count);
        Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize);
        Task<IEnumerable<Article>> GetByAuthorAsync(string authorId);
        Task<IEnumerable<Article>> GetAllBackendArticlesAsync();
        Task<Article?> GetByIdAsync(int id);
        Task<bool> AddAsync(Article article);
        Task UpdateAsync(Article article);
        Task<bool> SlugExistsAsync(string slug);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task IncrementViewCountAsync(int articleId);
        Task<bool> HasUserLikedArticleAsync(int articleId, string userId);
        Task AddLikeAsync(int articleId, string userId);
        Task RemoveLikeAsync(int articleId, string userId);
        Task<int> GetLikesCountAsync(int articleId);

        Task<IEnumerable<Article>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count);
        Task<IEnumerable<Article>> GetMostPopularByCategoryIdsAsync(List<int> categoryIds, int count);
        Task<IEnumerable<Article>> GetEditorChoiceByCategoryIdsAsync(List<int> categoryIds, int count);

        Task<IEnumerable<Article>> SearchArticlesAsync(string searchTerm);
    }
}