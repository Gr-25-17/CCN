using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;

namespace NewsSite.ViewComponents;

public class HomeSidebarVC : ViewComponent
{
    public IViewComponentResult Invoke(IEnumerable<ArticleSummaryViewModel> editorChoiceArticles, IEnumerable<ArticleSummaryViewModel> mostPopularArticles)
    {
        var model = new HomeSidebarViewModel
        {
            EditorChoiceArticles = editorChoiceArticles,
            MostPopularArticles = mostPopularArticles
        };

        return View(model);
    }
}
