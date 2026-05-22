using System.ComponentModel.DataAnnotations;

namespace NewsSite.Models.ViewModels;

public class UnsubscribeViewModel
{
    public string Token { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string CurrentFrequency { get; set; } = "Weekly";

    public List<string> SelectedCategories { get; set; } = new();

    public List<string> SelectedAuthors { get; set; } = new();

 
    [Display(Name = "New frequency")]
    public string? NewFrequency { get; set; }

    
    public List<int> SelectedCategoryIds { get; set; } = new();

    public List<string> SelectedAuthorIds { get; set; } = new();

  
    [Display(Name = "Reason for leaving (optional)")]
    public string? UnsubscribeReason { get; set; }


    public string Action { get; set; } = string.Empty;
}

public class UnsubscribeRequest
{
    public string Token { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; 
    public string? UnsubscribeReason { get; set; }
    public List<int> SelectedCategoryIds { get; set; } = new();
    public List<string> SelectedAuthorIds { get; set; } = new();
}