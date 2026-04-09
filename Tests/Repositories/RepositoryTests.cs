using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;

public class RepositoryTests
{
    private ApplicationDbContext GetContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task ArticleRepo_ToggleLike_ShouldAddAndRemove()
    {
        using var context = GetContext();
        var repo = new ArticleRepository(context);

        // Testar Add
        var (isLiked, count) = await repo.ToggleLikeAsync(1, "user1");
        isLiked.Should().BeTrue();
        count.Should().Be(1);

        // Testar Remove
        var (isLiked2, count2) = await repo.ToggleLikeAsync(1, "user1");
        isLiked2.Should().BeFalse();
        count2.Should().Be(0);
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