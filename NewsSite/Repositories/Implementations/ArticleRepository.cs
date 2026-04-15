using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations
{
    public class ArticleRepository(ApplicationDbContext context) : IArticleRepository
    {
        public async Task<Article?> GetBySlugAsync(string slug) => await context.Articles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .FirstOrDefaultAsync(a => a.Slug == slug && a.IsReadyForPublish && !a.IsDeleted && !a.IsArchived);

        public async Task<IEnumerable<Article>> GetLatestAsync(int count) => await context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetMostPopularAsync(int count) => await context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.ViewsCount)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count) => await context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted && a.IsEditorsChoice)
            .Include(a => a.Category)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize) => await context.Articles
            .Where(a => a.CategoryId == categoryId && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<IEnumerable<Article>> GetByAuthorAsync(string authorId) => await context.Articles
            .Where(a => a.AuthorId == authorId && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Article>> GetAllBackendArticlesAsync() => await context.Articles
            .Where(a => !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt).ToListAsync();

        public async Task<Article?> GetByIdAsync(int id) => await context.Articles
            .Include(a => a.Category)
            .Include(a => a.Author)
            .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<bool> AddAsync(Article article)
        {
            context.Articles.Add(article);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task UpdateAsync(Article article)
        {
            context.Articles.Update(article);
            await context.SaveChangesAsync();
        }

        public async Task<bool> SlugExistsAsync(string slug) => await context.Articles.AnyAsync(a => a.Slug == slug);

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync() => await context.Categories.ToListAsync();

        public async Task IncrementViewCountAsync(int articleId)
        {
            await context.Articles.Where(a => a.Id == articleId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewsCount, a => a.ViewsCount + 1));
        }

        public async Task<bool> HasUserLikedArticleAsync(int articleId, string userId) =>
            await context.ArticleLikes.AnyAsync(al => al.ArticleId == articleId && al.UserId == userId);

        public async Task AddLikeAsync(int articleId, string userId)
        {
            context.ArticleLikes.Add(new ArticleLike { ArticleId = articleId, UserId = userId });
            await context.SaveChangesAsync();
        }

        public async Task RemoveLikeAsync(int articleId, string userId)
        {
            var like = await context.ArticleLikes.FirstOrDefaultAsync(al => al.ArticleId == articleId && al.UserId == userId);
            if (like != null)
            {
                context.ArticleLikes.Remove(like);
                await context.SaveChangesAsync();
            }
        }

        public async Task<int> GetLikesCountAsync(int articleId) =>
            await context.ArticleLikes.CountAsync(al => al.ArticleId == articleId);

        // ToggleLikeAsync (din version)
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

        // Dina tre metoder
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