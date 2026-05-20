using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> HasActiveSubscriptionAsync(string userId)
        {
            return await _context.Subscriptions
                .AnyAsync(s => s.UserId == userId && s.EndDate > DateTime.UtcNow && s.PaymentComplete);
        }

        public async Task<Subscription?> GetLatestByUserIdAsync(string userId)
        {
            return await _context.Subscriptions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetDefaultSubscriptionTypeIdAsync()
        {
            var subscriptionType = await _context.SubscriptionTypes
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .FirstOrDefaultAsync();

            if (subscriptionType is null)
            {
                throw new InvalidOperationException("No subscription types exist in the database.");
            }

            return subscriptionType.Id;
        }

        public async Task SaveAsync(Subscription subscription)
        {
            if (subscription.Id == 0)
            {
                _context.Subscriptions.Add(subscription);
            }
            else
            {
                _context.Subscriptions.Update(subscription);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Subscription?> GetActiveSubscriptionAsync(string userId)
        {
            return await _context.Subscriptions
                .Include(s => s.Type)
                .Where(s => s.UserId == userId && s.EndDate > DateTime.UtcNow && s.PaymentComplete)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();
        }
    }
}
