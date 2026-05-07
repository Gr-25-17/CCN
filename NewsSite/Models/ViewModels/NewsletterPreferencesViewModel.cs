namespace NewsSite.Models.ViewModels;

public class NewsletterPreferencesViewModel
{
    public bool ReceiveNewsletter { get; set; } = false;

    public string Frequency { get; set; } = "Weekly";

    public string? SelectedCategoryIds { get; set; }

    public string? SelectedAuthorIds { get; set; }

    public List<CategoryViewModel> AvailableCategories { get; set; } = new();

    public List<AuthorViewModel> AvailableAuthors { get; set; } = new();

    public List<int> SelectedCategoryIdsList => SelectedCategoryIds?
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToList() ?? new List<int>();

    public List<string> SelectedAuthorIdsList => SelectedAuthorIds?
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .ToList() ?? new List<string>();
}
