using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces;

public interface IArticleService
{
    Task<IEnumerable<Article>> GetLatestAsync(int count);
    Task<IEnumerable<Article>> GetMostPopularAsync(int count);
    Task<IEnumerable<Article>> GetEditorChoiceAsync(int count);
    Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize);
}