using NewsSite.Models.Entities;

namespace NewsSite.Models.ViewModels;

/// <summary>
/// ViewModel for creating/editing newsletters in the admin panel
/// </summary>
public class NewsletterManagementViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Draft";

    public string? SelectedCategoryIds { get; set; }

    public int ArticlesPerCategory { get; set; } = 5;

    public int EditorChoiceCount { get; set; } = 3;

    public string? CustomHtmlHeader { get; set; }

    public string? CustomHtmlFooter { get; set; }

    public DateTime? ScheduledSendTime { get; set; }

    public DateTime? SentAt { get; set; }

    public int RecipientCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// List of all categories for dropdown selection
    /// </summary>
    public List<CategoryViewModel> AvailableCategories { get; set; } = new();

    /// <summary>
    /// Status options for the dropdown
    /// </summary>
    public static readonly List<string> StatusOptions = new() { "Draft", "Scheduled", "Sent", "Cancelled" };

    /// <summary>
    /// Get selected category IDs as a list of integers
    /// </summary>
    public List<int> GetSelectedCategoryIds()
    {
        if (string.IsNullOrEmpty(SelectedCategoryIds))
            return new List<int>();

        return SelectedCategoryIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
            .Where(id => id > 0)
            .ToList();
    }
}

/// <summary>
/// ViewModel for displaying a list of newsletters
/// </summary>
public class NewsletterListViewModel
{
    public List<NewsletterItemViewModel> Newsletters { get; set; } = new();
}

/// <summary>
/// ViewModel for displaying a single newsletter in a list
/// </summary>
public class NewsletterItemViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime? ScheduledSendTime { get; set; }

    public DateTime? SentAt { get; set; }

    public int RecipientCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedByName { get; set; } = string.Empty;
}
