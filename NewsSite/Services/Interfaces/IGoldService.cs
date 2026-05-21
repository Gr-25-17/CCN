namespace NewsSite.Services.Interfaces
{
    public interface IGoldService
    {
        Task<List<GoldPrice>> GetLatestPricesAsync(int count, string symbol = "Gold");
    }
}
