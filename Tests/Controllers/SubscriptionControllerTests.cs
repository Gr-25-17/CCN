using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace Tests.Controllers;

public class SubscriptionControllerTests
{
    private readonly Mock<ISubscriptionService> _subServiceMock;
    private readonly SubscriptionController _controller;

    public SubscriptionControllerTests()
    {
        _subServiceMock = new Mock<ISubscriptionService>();
        _controller = new SubscriptionController(_subServiceMock.Object);

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
        var model = CreateValidPaymentViewModel();
        _controller.ModelState.AddModelError("CardNumber", "Required");

        var result = await _controller.Index(model);

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeEquivalentTo(model);
    }

    [Fact]
    public async Task Index_Post_ShouldRedirectToHomeIndex_WhenValid()
    {
        var model = CreateValidPaymentViewModel();

        var result = await _controller.Index(model);

        var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
        redirectResult.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("Home");

        _subServiceMock.Verify(s => s.CreateOrRenewAsync("user123"), Times.Once);
    }

    private static PaymentViewModel CreateValidPaymentViewModel()
    {
        return new PaymentViewModel
        {
            CardName = "Test User",
            CardNumber = "4111111111111111",
            ExpirationDate = "12/30",
            CVV = "123"
        };
    }
}
