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
    public async Task UpdateRole_ShouldRedirectToPage_WhenSuccessful()
    {
        _userServiceMock.Setup(s => s.UpdateUserRoleAsync("user1", "Admin")).ReturnsAsync(true);

        var result = await _controller.UpdateRole("user1", "Admin");

        var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
    }

    [Fact]
    public async Task UpdateRole_ShouldSetErrorMessage_WhenServiceFails()
    {
        _userServiceMock.Setup(s => s.UpdateUserRoleAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

        await _controller.UpdateRole("bad-id", "Admin");

        _controller.TempData["Error"].Should().Be("Gick inte att uppdatera rollen.");
    }

    [Fact]
    public async Task SoftDelete_ShouldCallServiceWithCorrectId()
    {
        _userServiceMock.Setup(s => s.SoftDeleteUserAsync("user-to-delete")).ReturnsAsync(true);

        await _controller.SoftDelete("user-to-delete");

        _userServiceMock.Verify(s => s.SoftDeleteUserAsync("user-to-delete"), Times.Once);
    }
}