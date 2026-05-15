using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NewsSite.Models.Entities;

public class UnsubscribeLog
{
    [Key]
    public int Id { get; set; }


    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public string? Token { get; set; }

    public DateTime UnsubscribedAt { get; set; } = DateTime.UtcNow;

    public string? Reason { get; set; }

    public bool WasReactivated { get; set; } = false;  

    public DateTime? ReactivatedAt { get; set; }
}