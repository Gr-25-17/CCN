namespace NewsSite.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ArticleSummaryViewModel> LatestArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<ArticleSummaryViewModel> MostPopularArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<ArticleSummaryViewModel> EditorChoiceArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public bool HasActiveSubscription { get; set; } = false;
        public WeatherViewModel? Weather { get; set; }
        // Search results (optional). When IsSearch is true, the view will render SearchResults instead of LatestArticles.
        public IEnumerable<SearchArticleVM> SearchResults { get; set; } = new List<SearchArticleVM>();
        public bool IsSearch { get; set; } = false;
        public string? SearchTerm { get; set; }

        public IEnumerable<ArticleSummaryViewModel> PrioritizedArticles { get; set; } = new List<ArticleSummaryViewModel>();
    }
}
