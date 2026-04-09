using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using Xunit;
using System.Security.Claims;

namespace NewsSite.Tests.Controllers;

public class SubscriptionControllerTests
{
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly SubscriptionController _controller;

    public SubscriptionControllerTests()
    {
        _subServiceMock = new Mock<ISubscriptionService>();
        _controller = new SubscriptionController(_subServiceMock.Object);

        // Mocka HttpContext och User
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user123")
        }, "mock"));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Index_Post_ShouldReturnView_WhenModelStateIsInvalid()
    {
        var model = new PaymentViewModel { CardName = "Test" };
        _controller.ModelState.AddModelError("CardNumber", "Required");

        var result = await _controller.Index(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(model);
    }

    [Fact]
    public async Task Index_Post_ShouldRedirectToSuccess_WhenValid()
    {
        var model = new PaymentViewModel { CardName = "Valid User" };
        _subServiceMock.Setup(s => s.HasActiveSubscriptionAsync("user123")).ReturnsAsync(true);

        var result = await _controller.Index(model);

        result.Should().BeOfType<RedirectToActionResult>()
            .Which.ActionName.Should().Be("Success");
    }
}