namespace NewsSite.Models.Entities
{
    public class Subscription
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; }
        public bool PaymentComplete { get; set; } = false;

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int SubscriptionTypeId { get; set; }
        public SubscriptionType? Type { get; set; }
    }
}
