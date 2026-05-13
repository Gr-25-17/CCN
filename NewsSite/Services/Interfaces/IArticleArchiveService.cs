namespace NewsSite.Services.Interfaces;

public interface IArticleArchiveService
{
    Task<int> ArchiveArticlesOlderThanAsync(int days = 30);
}
