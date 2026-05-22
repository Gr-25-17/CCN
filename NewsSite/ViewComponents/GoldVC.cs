using Microsoft.AspNetCore.Mvc;
using NewsSite.Mapping;
using NewsSite.Models.ViewModels;
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
            var symbols = new[] { "Gold", "Silver", "Platinum", "Palladium" };
            var metalSeries = new Dictionary<string, List<GoldViewModel>>();

            foreach (var symbol in symbols)
            {
                var metalData = await _goldService.GetLatestPricesAsync(7, symbol);
                metalSeries[symbol] = metalData.Select(m => m.ToViewModel()).ToList();
            }

            var viewModel = new MetalPricesPanelViewModel
            {
                MetalSeries = metalSeries
            };

            return View(viewModel);
        }
    }
}
