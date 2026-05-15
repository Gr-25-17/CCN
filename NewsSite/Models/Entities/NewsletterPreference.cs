using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models.Entities;

public class NewsletterPreference
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public bool ReceiveNewsletter { get; set; } = false;

    public string Frequency { get; set; } = "Weekly";

    public string? SelectedCategoryIds { get; set; }
    public string? SelectedAuthIds { get; set; }

    public DateTime? LastSentDate { get; set; }

    public string? UnsubscribeToken { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsUnsubscribed { get; set; } = false;
    public DateTime? UnsubscribedAt { get; set; }
    public string? UnsubscribeReason { get; set; }


}