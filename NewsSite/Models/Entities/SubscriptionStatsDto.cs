namespace NewsSite.Models.Entities
{
    public class SubscriptionStatsDto
    {

        public int TotalRegisteredUsers { get; set; }
        public int ActiveSubscribers { get; set; }
        public int InactiveSubscribers { get; set; }

        public int NewSubscribersThisMonth { get; set; }
        public int NewSubscribersLastMonth { get; set; }

        public int RegistrationsThisMonth { get; set; }
        public int RegistrationsLastMonth { get; set; }

        public int ReturningSubscribers { get; set; }

        public double GrowthRateSubscribers { get; set; }
        public double GrowthRateRegistrations { get; set; }
        public double SubscriberPercentage { get; set; }
        public double ChurnRate { get; set; }

        public decimal EstimatedMonthlyRevenue { get; set; }

        public List<string> TrendLabels { get; set; } = [];
        public List<int> TrendSubscriberCounts { get; set; } = [];


    }
}
