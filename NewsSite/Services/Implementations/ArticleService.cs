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
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
    }
    

    public async Task<IEnumerable<Article>> GetMostPopularAsync(int count)
    {
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
       
    }

    public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize)
    {

        var skip = (page - 1) * pageSize;
        return await _context.Articles
            .Where(a => a.CategoryId == categoryId && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
            
    }

    public async Task<Article?> GetBySlugAsync(string slug)
    {
        return await _context.Articles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Slug == slug);
    }

    public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count)
    {
       
        return await _context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted && a.IsEditorsChoice)
            .Include(a => a.Category)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();
       
    }
}