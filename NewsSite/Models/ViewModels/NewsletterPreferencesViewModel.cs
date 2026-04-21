namespace NewsSite.Models.ViewModels;

public class NewsletterPreferencesViewModel
{
    public bool ReceiveNewsletter { get; set; } = false;

    public string Frequency { get; set; } = "Weekly";

    public string? SelectedCategoryIds { get; set; }

    public List<CategoryViewModel> AvailableCategories { get; set; } = new();
}
