using System.Net;
using Moq;
using Moq.Protected;
using NewsSite.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Services;

public class ResilienceTests
{
    [Fact]
    public async Task WeatherService_ShouldReturnEmptyModel_OnTimeout()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var client = new HttpClient(handlerMock.Object);
        var service = new WeatherService(client);

        var result = await service.GetWeatherAsync();

        result.Should().NotBeNull();
        result.City.Should().BeNull();
    }
}