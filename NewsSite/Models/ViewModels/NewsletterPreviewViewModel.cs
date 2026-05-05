using NewsSite.Models.Entities;

namespace NewsSite.Models.ViewModels;

/// <summary>
/// ViewModel for previewing a newsletter before sending
/// </summary>
public class NewsletterPreviewViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string HtmlContent { get; set; } = string.Empty;

    public int EstimatedRecipientCount { get; set; }

    public List<int> SelectedCategoryIds { get; set; } = new();

    public List<CategoryViewModel> SelectedCategories { get; set; } = new();

    public int TotalArticlesCount { get; set; }
}

/// <summary>
/// ViewModel for newsletter statistics
/// </summary>
public class NewsletterStatsViewModel
{
    public int TotalNewsletters { get; set; }

    public int DraftCount { get; set; }

    public int ScheduledCount { get; set; }

    public int SentCount { get; set; }

    public int CancelledCount { get; set; }

    public int TotalRecipients { get; set; }

    public DateTime? NextScheduledSend { get; set; }

    public DateTime? LastSent { get; set; }
}
