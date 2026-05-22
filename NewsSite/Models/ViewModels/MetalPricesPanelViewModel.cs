namespace NewsSite.Models.ViewModels
{
    public class MetalPricesPanelViewModel
    {
        public List<GoldViewModel> GoldPrices { get; set; } = [];
        public Dictionary<string, GoldViewModel?> SpotMetals { get; set; } = new();
    }
}
