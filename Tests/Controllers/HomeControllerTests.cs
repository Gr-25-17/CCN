using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;
using Tests.Helpers;

namespace Tests.Controllers;

public class HomeControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly Mock<ISubscriptionService> _subscriptionServiceMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<INewsletterService> _newsletterServiceMock;
    private readonly HomeController _controller;
    private readonly Mock<IWeatherService> _weatherServiceMock;
    private readonly Mock<ISubscriptionAnalyticsService> _subscriptionAnalyticsServiceMock;

    public HomeControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _subscriptionServiceMock = new Mock<ISubscriptionService>();
        _userManagerMock = IdentityMockHelper.MockUserManager<ApplicationUser>();
        _newsletterServiceMock = new Mock<INewsletterService>();
        _weatherServiceMock = new Mock<IWeatherService>();
        _subscriptionAnalyticsServiceMock = new Mock<ISubscriptionAnalyticsService>();


        _controller = new HomeController(
            _articleServiceMock.Object,
            _categoryServiceMock.Object,
            _subscriptionServiceMock.Object,
            _userManagerMock.Object,
            _newsletterServiceMock.Object,
            _weatherServiceMock.Object,
            _subscriptionAnalyticsServiceMock.Object);
            

        var user = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task Index_ShouldReturnViewWithModel()
    {
        _articleServiceMock.Setup(s => s.GetLatestAsync(It.IsAny<int>())).ReturnsAsync(new List<ArticleSummaryViewModel>());
        _articleServiceMock.Setup(s => s.GetMostPopularAsync(It.IsAny<int>())).ReturnsAsync(new List<ArticleSummaryViewModel>());
        _articleServiceMock.Setup(s => s.GetEditorChoiceAsync(It.IsAny<int>())).ReturnsAsync(new List<ArticleSummaryViewModel>());
        _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CategoryViewModel>());
        _newsletterServiceMock.Setup(s => s.GetPreferencesAsync(It.IsAny<string>()))
            .ReturnsAsync(new NewsletterPreferencesViewModel());

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        viewResult.Model.Should().BeOfType<HomeViewModel>();
    }
    [Fact]
    public async Task Index_WithSearchTerm_ShouldPopulateSidebarCollections()
    {
        var searchResults = new List<SearchArticleVM>
        {
            new() { Id = 1, Title = "Search hit", Slug = "search-hit" }
        };

        var editorChoice = new List<ArticleSummaryViewModel>
        {
            new() { Id = 2, Title = "Editor", Slug = "editor" }
        };

        var mostPopular = new List<ArticleSummaryViewModel>
        {
            new() { Id = 3, Title = "Popular", Slug = "popular" }
        };

        _articleServiceMock.Setup(s => s.SearchArticlesAsync("ai")).ReturnsAsync(searchResults);
        _articleServiceMock.Setup(s => s.GetEditorChoiceAsync(3)).ReturnsAsync(editorChoice);
        _articleServiceMock.Setup(s => s.GetMostPopularAsync(6)).ReturnsAsync(mostPopular);
        _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<CategoryViewModel>());
        _weatherServiceMock.Setup(s => s.GetWeatherAsync()).ReturnsAsync((NewsSite.Models.APIs.WeatherForecast?)null);

        _controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?searchTerm=ai");

        var result = await _controller.Index();

        var viewResult = result.Should().BeOfType<ViewResult>().Subject;
        var model = viewResult.Model.Should().BeOfType<HomeViewModel>().Subject;
        model.IsSearch.Should().BeTrue();
        model.SearchResults.Should().HaveCount(1);
        model.EditorChoiceArticles.Should().HaveCount(1);
        model.MostPopularArticles.Should().HaveCount(1);
    }

}
