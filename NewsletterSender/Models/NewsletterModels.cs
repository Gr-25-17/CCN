namespace NewsletterSender.Models;

/// <summary>
/// Represents a newsletter subscriber with preferences.
/// </summary>
public class Subscriber
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Comma-separated list of preferred category IDs (e.g., "1,2,3")
    /// </summary>
    public string PreferredCategoryIds { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string Locale { get; set; } = "en-US";
    public DateTime LastSentAt { get; set; }
    public string TemplateVersion { get; set; } = "1.0";
}

/// <summary>
/// Represents newsletter content (subject and HTML body).
/// </summary>
public class NewsletterContent
{
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
}

/// <summary>
/// Represents an article summary for the newsletter.
/// </summary>
public class NewsletterArticle
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsPremium { get; set; }
}

/// <summary>
/// Represents a delivery log entry.
/// </summary>
public class DeliveryLog
{
    public string NewsleterId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public string Status { get; set; } = string.Empty; // "Sent", "Failed", "Bounced"
    public string? ErrorMessage { get; set; }
}
