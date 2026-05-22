using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Areas.Identity.Pages.Account.Manage;
using NewsSite.Models.Entities;
using Tests.Helpers;
using FluentAssertions;
using System.Security.Claims;

namespace Tests.Areas.Identity;

public class TwoFactorAuthenticationTests
{
    [Fact]
    public async Task OnGetAsync_ShouldReturnNotFound_WhenUserIsNull()
    {
        var userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();
        userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((ApplicationUser?)null);

        var pageModel = new TwoFactorAuthenticationModel(userManagerMock.Object, null!, null!);

        var result = await pageModel.OnGetAsync();

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}