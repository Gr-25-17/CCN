namespace NewsSite.Models.ViewModels;

public class SubscriptionInfoViewModel
{
    public bool HasActiveSubscription { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? PlanName { get; set; }
    public decimal? Price { get; set; }
    public bool IsExpiringSoon => EndDate.HasValue && EndDate.Value <= DateTime.UtcNow.AddDays(7);
}