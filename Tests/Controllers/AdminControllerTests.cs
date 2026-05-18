using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using NewsSite.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Tests.Controllers;

public class AdminControllerTests
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly Mock<IImageOrchestrationService> _imageOrchestrationServiceMock;
    private readonly Mock<IArticleRepository> _articleRepositoryMock;
    private readonly Mock<ILocalToSqlServerMigrationService> _localToSqlServerMigrationServiceMock;
    private readonly Mock<ILogger<AdminController>> _loggerMock;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _userServiceMock = new Mock<IUserService>();
        _imageOrchestrationServiceMock = new Mock<IImageOrchestrationService>();
        _articleRepositoryMock = new Mock<IArticleRepository>();
        _localToSqlServerMigrationServiceMock = new Mock<ILocalToSqlServerMigrationService>();
        _loggerMock = new Mock<ILogger<AdminController>>();

        _controller = new AdminController(
            _userServiceMock.Object,
            _articleRepositoryMock.Object,
            _imageOrchestrationServiceMock.Object,
            _localToSqlServerMigrationServiceMock.Object,
            _loggerMock.Object);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    //[Fact]
    //public async Task Index_ShouldReturnViewWithUsers()
    //{
    //    // Din service-metod heter GetUsersForAdminAsync
    //    _userServiceMock.Setup(s => s.GetUsersForAdminAsync()).ReturnsAsync(new UserAdminViewModel());

    //    var result = await _controller.Index();

    //    var viewResult = result.Should().BeOfType<ViewResult>().Subject;
    //    viewResult.Model.Should().BeOfType<UserAdminViewModel>();
    //}

    //[Fact]
    //public async Task SoftDelete_ShouldRedirectWithErrorMessage_IfFails()
    //{
    //    _userServiceMock.Setup(s => s.SoftDeleteUserAsync(It.IsAny<string>())).ReturnsAsync(false);

    //    var result = await _controller.SoftDelete("bad-id");

    //    result.Should().BeOfType<RedirectToActionResult>();
    //    _controller.TempData["Error"].Should().Be("Kunde inte radera användaren.");
    //}
}
