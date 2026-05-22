using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class ArticleArchiveService(ApplicationDbContext dbContext) : IArticleArchiveService
{
    public async Task<int> ArchiveArticlesOlderThanAsync(int days = 30)
    {
        if (days <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(days));
        }

        var cutoffDate = DateTime.UtcNow.AddDays(-days);

        var articles = await dbContext.Articles
            .Where(a => !a.IsArchived && !a.IsDeleted && a.CreatedAt <= cutoffDate)
            .ToListAsync();

        if (articles.Count == 0)
        {
            return 0;
        }

        foreach (var article in articles)
        {
            article.IsArchived = true;
        }

        await dbContext.SaveChangesAsync();

        return articles.Count;
    }
}
