using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Services.Interfaces;


namespace NewsSite.ViewComponents
{
    public class GoldVC : ViewComponent
    {
        private readonly IGoldService _goldService;

        public GoldVC(IGoldService goldService)
        {
            _goldService = goldService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var rawData = await _goldService.GetLatestPricesAsync(7);

            // Using your extension method to convert to ViewModel
            var viewModel = rawData.Select(g => g.ToViewModel()).ToList();

            return View(viewModel);
        }
    }
}
