namespace NewsSite.Models.ViewModels
{
    public class MetalPricesPanelViewModel
    {
        public Dictionary<string, List<GoldViewModel>> MetalSeries { get; set; } = new();
    }
}
