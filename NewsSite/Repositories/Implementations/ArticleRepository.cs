using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations
{
    public class ArticleRepository(ApplicationDbContext context) : IArticleRepository
    {
        public async Task<Article?> GetBySlugAsync(string slug)
        {
            return await context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .FirstOrDefaultAsync(a => a.Slug == slug && a.IsReadyForPublish && !a.IsDeleted && !a.IsArchived);
        }
        public async Task<IEnumerable<Article>> GetLatestAsync(int count)
        {
            return await context.Articles
                .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetMostPopularAsync(int count)
        {
            return await context.Articles
                .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.ViewsCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count)
        {
            return await context.Articles
                .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted && a.IsEditorsChoice)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize)
        {
            var skip = (page - 1) * pageSize;
            return await context.Articles
                .Where(a => a.CategoryId == categoryId && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetByAuthorAsync(string authorId)
        {
            return await context.Articles
                .Where(a => a.AuthorId == authorId && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetAllBackendArticlesAsync()
        {
            return await context.Articles
                .Where(a => !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Article?> GetByIdAsync(int id)
        {
            return await context.Articles
                .Include(a => a.Category)
                .Include(a => a.Author)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task AddAsync(Article article)
        {
            context.Articles.Add(article);
            await context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Article article)
        {
            context.Articles.Update(article);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await context.Categories.ToListAsync();
        }
        public async Task IncrementViewCountAsync(int articleId)
        {
            var article = await context.Articles.FindAsync(articleId);
            if (article != null)
            {
                article.ViewsCount++;
                await context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId)
        {
            return await context.ArticleLikes
                .AnyAsync(al => al.ArticleId == articleId && al.UserId == userId);
        }

        public async Task<(bool IsLiked, int LikesCount)> ToggleLikeAsync(int articleId, string userId)
        {
            bool isLiked;
            var existingLike = await context.ArticleLikes
                .FirstOrDefaultAsync(al => al.ArticleId == articleId && al.UserId == userId);

            if (existingLike != null)
            {
                context.ArticleLikes.Remove(existingLike);
                isLiked = false;
            }
            else
            {
                context.ArticleLikes.Add(new ArticleLike
                {
                    ArticleId = articleId,
                    UserId = userId
                });
                isLiked = true;
            }

            await context.SaveChangesAsync();

            var likesCount = await context.ArticleLikes.CountAsync(al => al.ArticleId == articleId);

            return (isLiked, likesCount);
        }

        public async Task<IEnumerable<Article>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            return await context.Articles
                .Where(a => categoryIds.Contains(a.CategoryId) &&
                            a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetMostPopularByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            return await context.Articles
                .Where(a => categoryIds.Contains(a.CategoryId) &&
                            a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .OrderByDescending(a => a.ViewsCount)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> GetEditorChoiceByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            return await context.Articles
                .Where(a => categoryIds.Contains(a.CategoryId) &&
                            a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted &&
                            a.IsEditorsChoice)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }
    }
}