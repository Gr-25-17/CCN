using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Areas.Identity.Pages.Account.Manage;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;
using Tests.Helpers;
using FluentAssertions;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace NewsSite.Tests.Areas.Identity.Manage
{
    public class DeletePersonalDataTests
    {
        /*[Fact]
        public async Task OnPostAsync_ShouldCallAnonymizeAndSignOut()
        {
            var userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();
            var signInManagerMock = IdentityMockHelper.MockSignInManager(userManagerMock);
            var userServiceMock = new Mock<IUserService>();
            var loggerMock = new Mock<ILogger<DeletePersonalDataModel>>();

            var user = new ApplicationUser { Id = "u1" };
            userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
            userManagerMock.Setup(u => u.HasPasswordAsync(user)).ReturnsAsync(false);
            userServiceMock.Setup(s => s.AnonymizeUserAsync("u1")).ReturnsAsync(true);

            var pageModel = new DeletePersonalDataModel(
                userManagerMock.Object,
                signInManagerMock.Object,
                loggerMock.Object,
                userServiceMock.Object);

            var result = await pageModel.OnPostAsync();

            userServiceMock.Verify(s => s.AnonymizeUserAsync("u1"), Times.Once);
            signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
            result.Should().BeOfType<RedirectResult>();
            ((RedirectResult)result).Url.Should().Be("~/");
        }*/
    }
}