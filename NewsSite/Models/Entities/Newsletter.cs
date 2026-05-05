using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models.Entities;

/// <summary>
/// Represents a newsletter that can be created and sent to subscribers.
/// Allows admins to customize newsletter content, target audience, and scheduling.
/// </summary>
public class Newsletter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Newsletter status: Draft, Scheduled, Sent, Cancelled
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Scheduled, Sent, Cancelled

    /// <summary>
    /// Comma-separated category IDs to include in this newsletter.
    /// Empty means all categories.
    /// </summary>
    public string? SelectedCategoryIds { get; set; }

    /// <summary>
    /// Number of top articles to include from each category
    /// </summary>
    public int ArticlesPerCategory { get; set; } = 5;

    /// <summary>
    /// Number of editor choice articles to include
    /// </summary>
    public int EditorChoiceCount { get; set; } = 3;

    /// <summary>
    /// Custom HTML content to include at the top of the newsletter.
    /// Can be a banner, introduction, or promotional content.
    /// </summary>
    public string? CustomHtmlHeader { get; set; }

    /// <summary>
    /// Custom HTML content to include at the bottom of the newsletter.
    /// Can be disclaimers, additional links, or footer content.
    /// </summary>
    public string? CustomHtmlFooter { get; set; }

    /// <summary>
    /// When the newsletter is scheduled to be sent.
    /// Nullable for drafts or immediate sends.
    /// </summary>
    public DateTime? ScheduledSendTime { get; set; }

    /// <summary>
    /// When the newsletter was actually sent.
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Number of emails this newsletter was sent to
    /// </summary>
    public int RecipientCount { get; set; } = 0;

    /// <summary>
    /// ID of the admin who created this newsletter
    /// </summary>
    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    [ForeignKey("CreatedByUserId")]
    public ApplicationUser? CreatedBy { get; set; }

    /// <summary>
    /// When this newsletter was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this newsletter was last modified
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
