namespace NewsSite.Models.ViewModels
{
    public class GoldViewModel
    {
        public string DateLabel { get; set; } = string.Empty;
        public double Price { get; set; }
        public double Change { get; set; }
        public bool IsUp => Change >= 0;
        public string ChartColor => IsUp ? "#198754" : "#dc3545";


    }
}
