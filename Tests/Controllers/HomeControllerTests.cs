using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using NewsSite.Tests.Helpers;
using Xunit;

namespace NewsSite.Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly HomeController _controller;

    public HomeControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _subServiceMock = new Mock<ISubscriptionService>();
        _userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();

        // Här skickas alla 4 parametrar in i rätt ordning
        _controller = new HomeController(
            _articleServiceMock.Object,
            _categoryServiceMock.Object,
            _subServiceMock.Object,
            _userManagerMock.Object);
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithModel()
    {
        _articleServiceMock.Setup(s => s.GetLatestAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());
        _articleServiceMock.Setup(s => s.GetMostPopularAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());
        _articleServiceMock.Setup(s => s.GetEditorChoiceAsync(It.IsAny<int>())).ReturnsAsync(new List<Article>());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<HomeViewModel>();
    }
}