using System.Net;
using Moq;
using Moq.Protected;
using NewsSite.Services.Implementations;
using FluentAssertions;

namespace Tests.Services;

public class WeatherServiceTests
{
    [Fact]
    public async Task GetWeatherAsync_ShouldReturnEmptyObject_WhenApiIsDown()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var service = new WeatherService(httpClient);

        var result = await service.GetWeatherAsync();

        result.Should().NotBeNull();
        result.City.Should().BeNull();
    }
    [Fact]
    public async Task GetWeatherAsync_ShouldNotThrow_WhenApiIsUnreachable()
    {
        // Simulerar ett nätverksfel
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        var client = new HttpClient(handlerMock.Object);
        var service = new WeatherService(client);

        // Verifierar att metoden hanterar felet internt
        var act = async () => await service.GetWeatherAsync();
        await act.Should().NotThrowAsync();

        var result = await service.GetWeatherAsync();
        result.Should().NotBeNull();
    }
}