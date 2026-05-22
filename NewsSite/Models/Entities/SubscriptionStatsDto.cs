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
        public List<int> TrendUserCounts { get; set; } = [];

        public List<WriterPerformanceDto> WriterPerformances { get; set; } = [];
        public List<WriterMonthlyTrendDto> WriterMonthlyTrends { get; set; } = [];
    }

    public class WriterPerformanceDto
    {
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int ArticlesThisMonth { get; set; }
        public int TotalArticles { get; set; }
        public int TotalLikes { get; set; }
        public int TotalViews { get; set; }
        public double AvgEngagementPerArticle { get; set; }
        public decimal RevenueEstimate { get; set; }
        public double ImpactScore { get; set; }
    }

    public class WriterMonthlyTrendDto
    {
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string MonthLabel { get; set; } = string.Empty;
        public int Articles { get; set; }
        public int Engagement { get; set; }
    }
}
