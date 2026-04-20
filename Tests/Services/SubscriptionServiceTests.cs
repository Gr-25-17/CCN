using Moq;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using FluentAssertions;

namespace Tests.Services;

public class SubscriptionServiceTests
{
    [Fact]
    public async Task HasActiveSubscriptionAsync_ShouldReturnFalse_WhenUserIdIsEmpty()
    {
        var repoMock = new Mock<ISubscriptionRepository>();
        var service = new SubscriptionService(repoMock.Object);

        var result = await service.HasActiveSubscriptionAsync("");

        result.Should().BeFalse();
        repoMock.Verify(r => r.HasActiveSubscriptionAsync(It.IsAny<string>()), Times.Never);
    }
}