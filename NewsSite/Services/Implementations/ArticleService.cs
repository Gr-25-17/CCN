using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;

    public ArticleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Article>> GetLatestAsync(int count)
    {
        // TODO: Ta bort detta när databasen fungerar
        return await Task.FromResult(new List<Article>
        {
            new Article
            {
                Title = "Welcome to NewsSite!",
                Summary = "This is a demo article. Replace with real data when database is ready.",
                Category = new Category { Name = "Sweden" },
                CreatedAt = DateTime.Now,
                Slug = "welcome-to-newssite"
            },
            new Article
            {
                Title = "Second Demo Article",
                Summary = "Another demo article to show how the frontpage works.",
                Category = new Category { Name = "World" },
                CreatedAt = DateTime.Now,
                Slug = "second-demo-article"
            }
        });

        /* Original kod - avkommentera när databasen fungerar
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
        */
    }

    public async Task<IEnumerable<Article>> GetMostPopularAsync(int count)
    {
        // TODO: Ta bort detta när databasen fungerar
        return await Task.FromResult(new List<Article>
        {
            new Article
            {
                Title = "Popular Article 1",
                Summary = "This article is popular!",
                Category = new Category { Name = "Sport" },
                CreatedAt = DateTime.Now,
                Slug = "popular-article-1"
            },
            new Article
            {
                Title = "Popular Article 2",
                Summary = "Everyone is reading this!",
                Category = new Category { Name = "Sport" },
                CreatedAt = DateTime.Now,
                Slug = "popular-article-2"
            }
        });

        /* Original kod
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
        */
    }

    public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize)
    {
        // TODO: Ta bort detta när databasen fungerar
        var categoryName = categoryId switch
        {
            1 => "Sweden",
            2 => "World",
            3 => "Sport",
            _ => "General"
        };

        return await Task.FromResult(new List<Article>
        {
            new Article
            {
                Title = $"Demo article in {categoryName}",
                Summary = $"This is a demo article for the {categoryName} category.",
                Category = new Category { Name = categoryName },
                CreatedAt = DateTime.Now,
                Slug = $"demo-{categoryName.ToLower()}"
            }
        });

        /* Original kod
        var skip = (page - 1) * pageSize;
        return await _context.Articles
            .Where(a => a.CategoryId == categoryId && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        */
    }

    public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count)
    {
        // TODO: Ta bort detta när databasen fungerar
        return await Task.FromResult(new List<Article>
        {
            new Article
            {
                Title = "Editor's Choice: Featured Article",
                Summary = "This article is specially selected by our editors!",
                Category = new Category { Name = "World" },
                CreatedAt = DateTime.Now,
                Slug = "editors-choice-featured"
            }
        });

        /* Original kod
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted && a.IsEditorsChoice)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
        */
    }
}