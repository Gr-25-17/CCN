namespace NewsSite.Repositories.Interfaces
{
    public interface ISubscriptionRepository
    {
        Task<bool> HasActiveSubscriptionAsync(string userId);
    }
}
