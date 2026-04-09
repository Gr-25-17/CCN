using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

public class ArticlesControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly ArticlesController _controller;

    public ArticlesControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _controller = new ArticlesController(_articleServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "user-123")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task ToggleLike_ShouldReturnCorrectJson()
    {
        _articleServiceMock.Setup(s => s.ToggleLikeAsync(1, "user-123"))
            .ReturnsAsync((IsLiked: true, LikesCount: 10));

        var result = await _controller.ToggleLike(1);

        var json = result.Should().BeOfType<JsonResult>().Subject;
        var value = json.Value.ToString();
        value.Should().Contain("isLiked = True");
        value.Should().Contain("likesCount = 10");
    }

    [Fact]
    public async Task Details_ShouldReturnNotFound_WhenSlugIsEmpty()
    {
        var result = await _controller.Details("");
        result.Should().BeOfType<NotFoundResult>();
    }
}
