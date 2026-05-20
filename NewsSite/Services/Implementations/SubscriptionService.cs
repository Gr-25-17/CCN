using Microsoft.AspNetCore.Identity;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class SubscriptionService : ISubscriptionService
    {
        private const string SubscriberRole = "Subscriber";
        private const string ReaderRole = "Reader";
        private static readonly TimeSpan DefaultDuration = TimeSpan.FromDays(30);

        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SubscriptionService(
            ISubscriptionRepository subscriptionRepository,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _subscriptionRepository = subscriptionRepository;
            _userManager = userManager;
            _signInManager = signInManager;
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
            }
            else
            {
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

            var user = await _userManager.FindByIdAsync(userId)
                ?? throw new InvalidOperationException($"User '{userId}' was not found.");

            if (await _userManager.IsInRoleAsync(user, ReaderRole))
            {
                var removeResult = await _userManager.RemoveFromRoleAsync(user, ReaderRole);

                if (!removeResult.Succeeded)
                {
                    var errors = string.Join(", ", removeResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to remove Reader role: {errors}");
                }
            }

            if (!await _userManager.IsInRoleAsync(user, SubscriberRole))
            {
                var addResult = await _userManager.AddToRoleAsync(user, SubscriberRole);

                if (!addResult.Succeeded)
                {
                    var errors = string.Join(", ", addResult.Errors.Select(x => x.Description));
                    throw new InvalidOperationException($"Failed to assign Subscriber role: {errors}");
                }
            }

            await _signInManager.RefreshSignInAsync(user);
        }


    }

}
