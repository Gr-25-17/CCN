using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.Areas.Identity.Pages.Account.Manage;
using Xunit;

namespace NewsSite.Tests.Areas.Identity;

public class ShowRecoveryCodesTests
{
    [Fact]
    public void OnGet_ShouldRedirect_WhenNoCodesInTempData()
    {
        var pageModel = new ShowRecoveryCodesModel();
        pageModel.RecoveryCodes = null!; // Simulerar tom TempData

        var result = pageModel.OnGet();

        var redirect = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirect.PageName.Should().Be("./TwoFactorAuthentication");
    }

    [Fact]
    public void OnGet_ShouldReturnPage_WhenCodesExist()
    {
        var pageModel = new ShowRecoveryCodesModel();
        pageModel.RecoveryCodes = new[] { "code1", "code2" };

        var result = pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
    }
}