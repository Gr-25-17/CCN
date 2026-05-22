namespace NewsSite.Models.ViewModels
{
    public class HomeSidebarViewModel
    {
        public IEnumerable<ArticleSummaryViewModel> EditorChoiceArticles { get; set; } = [];
        public IEnumerable<ArticleSummaryViewModel> MostPopularArticles { get; set; } = [];
    }
}
