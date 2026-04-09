using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace Tests.Services
{
    public class ArticlesControllerTests
    {
        private readonly Mock<IArticleService> _articleServiceMock;
        private readonly Mock<ISubscriptionService> _subServiceMock;
        private readonly ArticlesController _controller;

        public ArticlesControllerTests()
        {
            _articleServiceMock = new Mock<IArticleService>();
            _subServiceMock = new Mock<ISubscriptionService>();
            _controller = new ArticlesController(_articleServiceMock.Object);
        }
        [Fact]
        public async Task Details_ShouldReturnNotFound_WhenArticleDoesNotExist()
        {
            _articleServiceMock.Setup(s => s.GetBySlugAsync(It.IsAny<string>())).ReturnsAsync((Article?)null);

            var res = await _controller.Details("eafeaf");
            res.Should().BeOfType<NotFoundResult>();
        }
    }
}
