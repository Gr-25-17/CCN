using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private static readonly TimeSpan DefaultDuration = TimeSpan.FromDays(30);
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriptionService(ISubscriptionRepository subscriptionRepository)
        {
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
            => !string.IsNullOrWhiteSpace(userId) && await _subscriptionRepository.HasActiveSubscriptionAsync(userId);

        public async Task CreateOrRenewAsync(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);

            var existing = await _subscriptionRepository.GetLatestByUserIdAsync(userId);
            var startDate = DateTime.UtcNow;

            if (existing is not null)
            {
                existing.StartDate = existing.EndDate > startDate ? existing.StartDate : startDate;
                existing.EndDate = (existing.EndDate > startDate ? existing.EndDate : startDate).Add(DefaultDuration);
                existing.PaymentComplete = true;
                existing.RenewalReminderSentAt = null;

                await _subscriptionRepository.SaveAsync(existing);
                return;
            }

            var subscriptionTypeId = await _subscriptionRepository.GetDefaultSubscriptionTypeIdAsync(); 

            var subscription = new Subscription
            {
                UserId = userId,
                SubscriptionTypeId = subscriptionTypeId,
                StartDate = startDate,
                EndDate = startDate.Add(DefaultDuration),
                PaymentComplete = true,
                RenewalReminderSentAt = null
            };

            await _subscriptionRepository.SaveAsync(subscription);
        }
    }
}
