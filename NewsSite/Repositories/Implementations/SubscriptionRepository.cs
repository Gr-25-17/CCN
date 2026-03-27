using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
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
                .AnyAsync(s => s.UserId == userId && s.EndDate > DateTime.Now && s.PaymentComplete == true);
        }
    }
}
