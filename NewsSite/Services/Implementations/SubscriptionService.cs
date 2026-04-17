using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }


        public async Task<bool> HasActiveSubscriptionAsync(string userId)
            => !string.IsNullOrWhiteSpace(userId) && await _subscriptionRepository.HasActiveSubscriptionAsync(userId);
        
    }
}
