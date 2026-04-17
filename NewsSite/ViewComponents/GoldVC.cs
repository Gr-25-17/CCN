using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Implementations;


namespace NewsSite.ViewComponents
{
    public class GoldVC : ViewComponent
    {
        private readonly GoldService _goldService;

        public GoldVC(GoldService goldService)
        {
            _goldService = goldService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rawData = await _goldService.GetLatestPricesAsync();

            // Using your extension method to convert to ViewModel
            var viewModel = rawData.Select(g => g.ToViewModel()).ToList();

            return View(viewModel);
        }
    }
}
