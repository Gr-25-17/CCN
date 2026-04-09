using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;

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
        _userServiceMock.Setup(s => s.GetUsersForAdminAsync()).ReturnsAsync(new UserAdminViewModel());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<UserAdminViewModel>();
    }

    [Fact]
    public async Task UpdateRole_ShouldSetError_WhenUpdateFails()
    {
        _userServiceMock.Setup(s => s.UpdateUserRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        await _controller.UpdateRole("invalid-id", "Admin");

        _controller.TempData["Error"].Should().Be("Gick inte att uppdatera rollen.");
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