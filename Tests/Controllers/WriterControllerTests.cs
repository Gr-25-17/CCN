using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using Xunit;
using System.Security.Claims;

namespace Tests.Controllers;

public class WriterControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly Mock<IBlobService> _blobServiceMock;
    private readonly Mock<IHttpClientFactory> _httpMock;
    private readonly WriterController _controller;

    public WriterControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _blobServiceMock = new Mock<IBlobService>();
        _httpMock = new Mock<IHttpClientFactory>();
        _controller = new WriterController(_articleServiceMock.Object, _blobServiceMock.Object, _httpMock.Object);
    }

    [Fact]
    public async Task Index_ShouldCallServiceWithUserContext()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, "writer-1")
        }, "mock"));
        _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        _articleServiceMock.Setup(s => s.GetBackendArticlesAsync("writer-1", false))
            .ReturnsAsync(new List<ArticleViewModel>());

        await _controller.Index();

        _articleServiceMock.Verify(s => s.GetBackendArticlesAsync("writer-1", false), Times.Once);
    }
}
