namespace NewsSite.Models.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool PaymentComplete { get; set; }
        public DateTime? RenewalReminderSentAt { get; set; }

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int SubscriptionTypeId { get; set; }
        public SubscriptionType? Type { get; set; }
    }
}
