using NewsSite.Models.Entities;

namespace NewsSite.Models.ViewModels;

public class HomeViewModel
{
    public IEnumerable<Article> LatestArticles { get; set; } = new List<Article>();
    public IEnumerable<Article> MostPopularArticles { get; set; } = new List<Article>();
    public IEnumerable<Article> EditorChoiceArticles { get; set; } = new List<Article>();
    public IEnumerable<Category> Categories { get; set; } = new List<Category>();

    public bool HasActiveSubscription { get; set; } = false;
}