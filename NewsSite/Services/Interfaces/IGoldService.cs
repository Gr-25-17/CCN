using NewsSite.Models.APIs;

namespace NewsSite.Services.Interfaces
{
    public interface IGoldService
    {
        Task<List<GoldPrice>> GetLatestPricesAsync(int count);
    }
}
