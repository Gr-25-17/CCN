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
            var goldRawData = await _goldService.GetLatestPricesAsync(7, "Gold");

            var spotSymbols = new[] { "Silver", "Platinum", "Palladium" };
            var spotMetals = new Dictionary<string, GoldViewModel?>();

            foreach (var symbol in spotSymbols)
            {
                var metalData = await _goldService.GetLatestPricesAsync(1, symbol);
                spotMetals[symbol] = metalData.Select(m => m.ToViewModel()).FirstOrDefault();
            }

            var viewModel = new MetalPricesPanelViewModel
            {
                GoldPrices = goldRawData.Select(g => g.ToViewModel()).ToList(),
                SpotMetals = spotMetals
            };

            return View(viewModel);
        }
    }
}
