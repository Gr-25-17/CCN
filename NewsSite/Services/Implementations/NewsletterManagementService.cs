using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Mapping;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class NewsletterManagementService : INewsletterManagementService
{
    private readonly INewsletterRepository _newsletterRepo;
    private readonly ICategoryService _categoryService;
    private readonly IArticleService _articleService;
    private readonly ApplicationDbContext _context;

    public NewsletterManagementService(
        INewsletterRepository newsletterRepo,
        ICategoryService categoryService,
        IArticleService articleService,
        ApplicationDbContext context)
    {
        _newsletterRepo = newsletterRepo;
        _categoryService = categoryService;
        _articleService = articleService;
        _context = context;
    }

    public async Task<List<NewsletterItemViewModel>> GetAllNewslettersAsync()
    {
        var newsletters = await _newsletterRepo.GetAllAsync();

        return newsletters.Select(n => new NewsletterItemViewModel
        {
            Id = n.Id,
            Title = n.Title,
            Status = n.Status,
            ScheduledSendTime = n.ScheduledSendTime,
            SentAt = n.SentAt,
            RecipientCount = n.RecipientCount,
            CreatedAt = n.CreatedAt,
            CreatedByName = n.CreatedBy?.UserName ?? "Unknown"
        }).ToList();
    }

    public async Task<NewsletterManagementViewModel?> GetNewsletterForEditAsync(int id)
    {
        var newsletter = await _newsletterRepo.GetByIdAsync(id);
        if (newsletter == null)
            return null;

        var categories = await _categoryService.GetAllAsync();

        return new NewsletterManagementViewModel
        {
            Id = newsletter.Id,
            Title = newsletter.Title,
            Description = newsletter.Description,
            Status = newsletter.Status,
            SelectedCategoryIds = newsletter.SelectedCategoryIds,
            ArticlesPerCategory = newsletter.ArticlesPerCategory,
            EditorChoiceCount = newsletter.EditorChoiceCount,
            CustomHtmlHeader = newsletter.CustomHtmlHeader,
            CustomHtmlFooter = newsletter.CustomHtmlFooter,
            ScheduledSendTime = newsletter.ScheduledSendTime,
            SentAt = newsletter.SentAt,
            RecipientCount = newsletter.RecipientCount,
            CreatedAt = newsletter.CreatedAt,
            UpdatedAt = newsletter.UpdatedAt,
            AvailableCategories = categories.ToList()
        };
    }

    public async Task<NewsletterManagementViewModel> CreateNewsletterAsync(NewsletterManagementViewModel viewModel, string userId)
    {
        var newsletter = new Newsletter
        {
            Title = viewModel.Title,
            Description = viewModel.Description,
            Status = "Draft",
            SelectedCategoryIds = viewModel.SelectedCategoryIds,
            ArticlesPerCategory = viewModel.ArticlesPerCategory,
            EditorChoiceCount = viewModel.EditorChoiceCount,
            CustomHtmlHeader = viewModel.CustomHtmlHeader,
            CustomHtmlFooter = viewModel.CustomHtmlFooter,
            ScheduledSendTime = viewModel.ScheduledSendTime,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var created = await _newsletterRepo.CreateAsync(newsletter);

        viewModel.Id = created.Id;
        viewModel.Status = created.Status;
        viewModel.CreatedAt = created.CreatedAt;
        viewModel.UpdatedAt = created.UpdatedAt;

        return viewModel;
    }

    public async Task<NewsletterManagementViewModel> UpdateNewsletterAsync(NewsletterManagementViewModel viewModel)
    {
        var newsletter = await _newsletterRepo.GetByIdAsync(viewModel.Id);
        if (newsletter == null)
            throw new InvalidOperationException($"Newsletter {viewModel.Id} not found");

        newsletter.Title = viewModel.Title;
        newsletter.Description = viewModel.Description;
        newsletter.Status = viewModel.Status;
        newsletter.SelectedCategoryIds = viewModel.SelectedCategoryIds;
        newsletter.ArticlesPerCategory = viewModel.ArticlesPerCategory;
        newsletter.EditorChoiceCount = viewModel.EditorChoiceCount;
        newsletter.CustomHtmlHeader = viewModel.CustomHtmlHeader;
        newsletter.CustomHtmlFooter = viewModel.CustomHtmlFooter;
        newsletter.ScheduledSendTime = viewModel.ScheduledSendTime;
        newsletter.UpdatedAt = DateTime.UtcNow;

        var updated = await _newsletterRepo.UpdateAsync(newsletter);

        viewModel.UpdatedAt = updated.UpdatedAt;

        return viewModel;
    }

    public async Task DeleteNewsletterAsync(int id)
    {
        await _newsletterRepo.SoftDeleteAsync(id);
    }

    public async Task<NewsletterPreviewViewModel> GetNewsletterPreviewAsync(int id)
    {
        var newsletter = await _newsletterRepo.GetByIdAsync(id);
        if (newsletter == null)
            throw new InvalidOperationException($"Newsletter {id} not found");

        var categories = await _categoryService.GetAllAsync();
        var selectedCategoryIds = newsletter.GetSelectedCategoryIds();

        var selectedCategories = categories
            .Where(c => selectedCategoryIds.Contains(c.Id))
            .ToList();

        // Fetch sample articles
        var articles = await _articleService.GetLatestAsync(newsletter.ArticlesPerCategory + newsletter.EditorChoiceCount);
        var articlesList = articles.ToList();

        var htmlContent = GeneratePreviewHtml(newsletter, articlesList);

        return new NewsletterPreviewViewModel
        {
            Id = newsletter.Id,
            Title = newsletter.Title,
            HtmlContent = htmlContent,
            EstimatedRecipientCount = await GetEstimatedRecipientCountAsync(selectedCategoryIds),
            SelectedCategoryIds = selectedCategoryIds,
            SelectedCategories = selectedCategories.ToList(),
            TotalArticlesCount = articlesList.Count
        };
    }

    public async Task<bool> SendNewsletterNowAsync(int id)
    {
        var newsletter = await _newsletterRepo.GetByIdAsync(id);
        if (newsletter == null)
            return false;

        // Mark as scheduled for immediate sending
        newsletter.Status = "Scheduled";
        newsletter.ScheduledSendTime = DateTime.UtcNow;
        await _newsletterRepo.UpdateAsync(newsletter);

        return true;
    }

    public async Task<NewsletterStatsViewModel> GetStatsAsync()
    {
        var newsletters = await _newsletterRepo.GetActiveAsync();

        var stats = new NewsletterStatsViewModel
        {
            TotalNewsletters = newsletters.Count,
            DraftCount = newsletters.Count(n => n.Status == "Draft"),
            ScheduledCount = newsletters.Count(n => n.Status == "Scheduled"),
            SentCount = newsletters.Count(n => n.Status == "Sent"),
            CancelledCount = newsletters.Count(n => n.Status == "Cancelled"),
            TotalRecipients = newsletters.Sum(n => n.RecipientCount),
            NextScheduledSend = newsletters
                .Where(n => n.Status == "Scheduled" && n.ScheduledSendTime.HasValue)
                .OrderBy(n => n.ScheduledSendTime)
                .FirstOrDefault()
                ?.ScheduledSendTime,
            LastSent = newsletters
                .Where(n => n.Status == "Sent" && n.SentAt.HasValue)
                .OrderByDescending(n => n.SentAt)
                .FirstOrDefault()
                ?.SentAt
        };

        return stats;
    }

    private string GeneratePreviewHtml(Newsletter newsletter, IEnumerable<ArticleSummaryViewModel> articles)
    {
        var baseUrl = "https://newssite.com";
        var articlesList = articles.Take(newsletter.ArticlesPerCategory + newsletter.EditorChoiceCount).ToList();

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{System.Web.HttpUtility.HtmlEncode(newsletter.Title)}</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; text-align: center; border-radius: 5px; }}
        .article {{ margin-bottom: 20px; padding: 15px; border-left: 4px solid #667eea; }}
        .article h3 {{ margin-top: 0; color: #333; }}
        .article p {{ margin: 10px 0; color: #666; font-size: 14px; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>{System.Web.HttpUtility.HtmlEncode(newsletter.Title)}</h1>
        </div>";

        if (!string.IsNullOrEmpty(newsletter.CustomHtmlHeader))
        {
            html += $@"<div style='margin: 20px 0;'>{newsletter.CustomHtmlHeader}</div>";
        }

        html += "<div style='margin: 20px 0;'>";

        foreach (var article in articlesList)
        {
            html += $@"
        <div class='article'>
            <h3>{System.Web.HttpUtility.HtmlEncode(article.Title)}</h3>
            <p><strong>Author:</strong> {System.Web.HttpUtility.HtmlEncode(article.AuthorName)}</p>
            <p>{System.Web.HttpUtility.HtmlEncode(article.Summary ?? "No summary available")}</p>
            <a href='{baseUrl}/articles/{article.Slug}' style='color: #667eea; text-decoration: none;'>Read More →</a>
        </div>";
        }

        html += "</div>";

        if (!string.IsNullOrEmpty(newsletter.CustomHtmlFooter))
        {
            html += $@"<div style='margin: 20px 0;'>{newsletter.CustomHtmlFooter}</div>";
        }

        html += @"
        <div class='footer'>
            <p>You're receiving this because you subscribed to our newsletter.</p>
            <p><a href='#' style='color: #667eea;'>Manage Preferences</a> | <a href='#' style='color: #667eea;'>Unsubscribe</a></p>
        </div>
    </div>
</body>
</html>";

        return html;
    }

    private async Task<int> GetEstimatedRecipientCountAsync(List<int> categoryIds)
    {
        var query = _context.NewsletterPreferences.Where(np => np.ReceiveNewsletter);

        if (categoryIds.Any())
        {
            query = query.Where(np => 
                np.SelectedCategoryIds == null || 
                np.SelectedCategoryIds == "" ||
                categoryIds.Any(cid => np.SelectedCategoryIds.Contains(cid.ToString())));
        }

        return await query.CountAsync();
    }
}
