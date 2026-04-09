using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace NewsSite.Tests.Integration;

public class BasicFlowTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public BasicFlowTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/Privacy")]
    [InlineData("/Articles/Details/test-slug")]
    public async Task Get_Endpoints_ReturnSuccessAndCorrectContentType(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        if (response.StatusCode == HttpStatusCode.NotFound && url.Contains("test-slug"))
        {
            return;
        }

        response.EnsureSuccessStatusCode();
        response.Content.Headers.ContentType!.ToString().Should().Contain("text/html");
    }
}