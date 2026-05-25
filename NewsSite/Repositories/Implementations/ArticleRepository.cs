using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
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
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetMostPopularAsync(int count) => await context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.ViewsCount)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetEditorChoiceAsync(int count) => await context.Articles
            .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted && a.IsEditorsChoice)
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count).ToListAsync();

        public async Task<IEnumerable<Article>> GetByCategoryAsync(int categoryId, int page, int pageSize) => await context.Articles
            .Where(a => a.CategoryId == categoryId && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Author)
            .Include(a => a.Likes)
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        public async Task<IEnumerable<Article>> GetByAuthorAsync(string authorId) => await context.Articles
            .Where(a => a.AuthorId == authorId && !a.IsDeleted)
            .Include(a => a.Category)
            .Include(a => a.Author)
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


        public async Task<IEnumerable<Article>> GetLatestByCategoryIdsAsync(List<int> categoryIds, int count)
        {
            return await context.Articles
                .Where(a => categoryIds.Contains(a.CategoryId) &&
                            a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Author)
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
                .Include(a => a.Author)
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
                .Include(a => a.Author)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Article>> SearchArticlesAsync(string searchItem)
        {
            if (string.IsNullOrWhiteSpace(searchItem))
            {
                return new List<Article>();
            }

            var item = searchItem.ToLower();

            // Find articles where title, summary, content or author matches the search term.
            // We fetch matching rows from the database and then apply a simple relevance
            // score in memory so results containing the term in the title rank higher,
            // then author matches, then summary/content matches.
            var query = context.Articles
                .Where(a => !a.IsDeleted && a.IsReadyForPublish && !a.IsArchived &&
                    (a.Title.ToLower().Contains(item)
                     || a.Summary.ToLower().Contains(item)
                     || a.Content.ToLower().Contains(item)
                     || (a.AuthorName != null && a.AuthorName.ToLower().Contains(item))
                     || (a.Author != null && (a.Author.FirstName + " " + a.Author.LastName).ToLower().Contains(item))
                    ))
                .Include(a => a.Category)
                .Include(a => a.Author);

            var list = await query.ToListAsync();

            var ordered = list
                .Select(a => new
                {
                    Article = a,
                    Score = (a.Title != null && a.Title.ToLower().Contains(item) ? 3 : 0)
                            + ((a.AuthorName != null && a.AuthorName.ToLower().Contains(item)) || (a.Author != null && (a.Author.FirstName + " " + a.Author.LastName).ToLower().Contains(item)) ? 2 : 0)
                            + ((a.Summary != null && a.Summary.ToLower().Contains(item)) || (a.Content != null && a.Content.ToLower().Contains(item)) ? 1 : 0)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Article.CreatedAt)
                .Select(x => x.Article)
                .ToList();

            return ordered;
        }

        public IQueryable<Article> GetQueryable() => context.Articles.AsQueryable();

        public async Task<IEnumerable<ArticleSummaryViewModel>> GetAllArticlesSortedByPreferencesAsync(List<int> preferredCategoryIds, List<string> preferredAuthorIds, int count)
        {
            var articles = await context.Articles
                .Where(a => a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Likes)
                .ToListAsync();

            
            var sortedArticles = articles
                .OrderByDescending(a => preferredCategoryIds.Contains(a.CategoryId) ||
                                        preferredAuthorIds.Contains(a.AuthorId))
                .ThenByDescending(a => a.IsEditorsChoice)
                .ThenByDescending(a => a.ViewsCount)
                .ThenByDescending(a => a.CreatedAt)
                .Take(count)
                .Select(a => new ArticleSummaryViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Slug = a.Slug,
                    Summary = a.Summary,
                    ImageUrl = a.ImageUrl,
                    CreatedAt = a.CreatedAt,
                    CategoryName = a.Category?.Name ?? "",
                    ViewsCount = a.ViewsCount,
                    LikesCount = a.Likes?.Count ?? 0,
                    IsEditorsChoice = a.IsEditorsChoice
                })
                .ToList();


            return sortedArticles;
        }
        public async Task<IEnumerable<Article>> GetArticlesToArchiveAsync(int days)
        {
            var archiveDate = DateTime.UtcNow.AddDays(-days);
            return await context.Articles
                .Where(a => !a.IsArchived &&
                            !a.IsDeleted &&
                            !a.IsPremium &&
                            !a.IsEditorsChoice &&
                            a.CreatedAt < archiveDate)
                .ToListAsync();
        }

        public async Task ArchiveArticlesAsync(List<int> ids)
        {
            await context.Articles
                .Where(a => ids.Contains(a.Id))
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsArchived, true));
        }

        public async Task SoftDeleteAsync(Article article)
        {
            article.IsDeleted = true;
            context.Articles.Update(article);
            await context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Article>> GetPremiumByCategoryAsync(int categoryId, int count) => await context.Articles
                .Where(a => a.CategoryId == categoryId && a.IsPremium && a.IsReadyForPublish && !a.IsArchived && !a.IsDeleted)
                .Include(a => a.Category)
                .Include(a => a.Author)
                .Include(a => a.Likes)
                .OrderByDescending(a => a.CreatedAt)
                .Take(count).ToListAsync();
    }
}
