using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;

namespace NewsSite.Tests.Repositories
{
    public class SubscriptionRepositoryTests
    {
        private ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task SubscriptionRepo_ShouldCheckEndDate()
        {
            using var context = GetContext();
            context.Subscriptions.Add(new Subscription { UserId = "u1", EndDate = DateTime.Now.AddDays(1), PaymentComplete = true });
            context.Subscriptions.Add(new Subscription { UserId = "u2", EndDate = DateTime.Now.AddDays(-1), PaymentComplete = true });
            await context.SaveChangesAsync();

            var repo = new SubscriptionRepository(context);
            (await repo.HasActiveSubscriptionAsync("u1")).Should().BeTrue();
            (await repo.HasActiveSubscriptionAsync("u2")).Should().BeFalse();
        }
    }
}
