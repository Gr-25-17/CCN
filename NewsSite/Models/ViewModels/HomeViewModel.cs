namespace NewsSite.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ArticleSummaryViewModel> LatestArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<ArticleSummaryViewModel> MostPopularArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<ArticleSummaryViewModel> EditorChoiceArticles { get; set; } = new List<ArticleSummaryViewModel>();
        public IEnumerable<CategoryViewModel> Categories { get; set; } = new List<CategoryViewModel>();
        public bool HasActiveSubscription { get; set; } = false;
    }
}
