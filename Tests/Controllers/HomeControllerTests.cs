using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;
using Tests.Helpers;
using Xunit;

namespace NewsSite.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();

        _controller = new HomeController(
            _articleServiceMock.Object,
            _categoryServiceMock.Object,
            _subscriptionServiceMock.Object,
            _userManagerMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithModel()
    {
        // Mocka alla anrop som görs i Index-metoden
        _articleServiceMock.Setup(s => s.GetLatestAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());
        _articleServiceMock.Setup(s => s.GetMostPopularAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());
        _articleServiceMock.Setup(s => s.GetEditorChoiceAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());

        // VIKTIGT: Mocka även kategoritjänsten så att vyn inte får null
        _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Category>());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<HomeViewModel>();
    }
}