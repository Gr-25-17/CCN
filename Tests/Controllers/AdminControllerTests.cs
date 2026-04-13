using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace NewsSite.Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _controller = new AdminController(_userServiceMock.Object);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithUsers()
    {
        // Din service-metod heter GetUsersForAdminAsync
        _userServiceMock.Setup(s => s.GetUsersForAdminAsync()).ReturnsAsync(new UserAdminViewModel());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<UserAdminViewModel>();
    }

    [Fact]
    public async Task SoftDelete_ShouldRedirectWithErrorMessage_IfFails()
    {
        _userServiceMock.Setup(s => s.SoftDeleteUserAsync(It.IsAny<string>())).ReturnsAsync(false);

        var result = await _controller.SoftDelete("bad-id");

        result.Should().BeOfType<RedirectToActionResult>();
        _controller.TempData["Error"].Should().Be("Kunde inte radera användaren.");
    }
}