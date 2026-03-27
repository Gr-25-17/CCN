namespace NewsSite.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
    }
}
