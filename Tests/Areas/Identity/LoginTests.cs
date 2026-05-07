using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using NewsSite.Areas.Identity.Pages.Account;
using NewsSite.Models.Entities;
using Tests.Helpers;
using FluentAssertions;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Tests.Areas.Identity
{
    public class LoginTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
        private readonly Mock<ILogger<LoginModel>> _loggerMock;
        private readonly LoginModel _pageModel;

        public LoginTests()
        {
            _userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();
            _signInManagerMock = IdentityMockHelper.MockSignInManager(_userManagerMock);
            _loggerMock = new Mock<ILogger<LoginModel>>();

            var urlHelperMock = new Mock<IUrlHelper>();
            urlHelperMock.Setup(x => x.Content(It.IsAny<string>())).Returns((string url) => url);

            _signInManagerMock.Setup(s => s.GetExternalAuthenticationSchemesAsync())
                .ReturnsAsync(new List<AuthenticationScheme>());

            _pageModel = new LoginModel(_signInManagerMock.Object, _loggerMock.Object, _userManagerMock.Object)
            {
                PageContext = new PageContext(),
                Url = urlHelperMock.Object,
                Input = new LoginModel.InputModel
                {
                    Email = "test@test.com",
                    Password = "Password123!"
                }
            };
        }

        /*[Fact]
        public async Task OnPostAsync_ShouldReturnLocalRedirect_WhenLoginSucceeds()
        {
            var user = new ApplicationUser { Email = "test@test.com", IsDeleted = false };
            _userManagerMock.Setup(u => u.FindByEmailAsync("test@test.com")).ReturnsAsync(user);

            _signInManagerMock.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), false))
                .ReturnsAsync(SignInResult.Success);

            var result = await _pageModel.OnPostAsync();

            result.Should().BeOfType<LocalRedirectResult>();
        }

        [Fact]
        public async Task OnPostAsync_ShouldReturnPage_WhenUserIsDeleted()
        {
            var user = new ApplicationUser { Email = "test@test.com", IsDeleted = true };
            _userManagerMock.Setup(u => u.FindByEmailAsync("test@test.com")).ReturnsAsync(user);

            var result = await _pageModel.OnPostAsync();

            result.Should().BeOfType<PageResult>();
            _pageModel.ModelState.IsValid.Should().BeFalse();
            _pageModel.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Contain("inaktiverat");
        }*/
    }
}