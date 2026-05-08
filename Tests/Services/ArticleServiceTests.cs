using NewsSite.Services.Implementations;
using FluentAssertions;
using Moq;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;

namespace Tests.Services;

public class ArticleServiceTests
{
    private readonly Mock<IArticleRepository> _repoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly ArticleService _service;

        public ArticleServiceTests()
        {
            _repoMock = new Mock<IArticleRepository>();
            _userRepoMock = new Mock<IUserRepository>();
            _service = new ArticleService(_repoMock.Object, _userRepoMock.Object);
        }

    [Theory]
    [InlineData("MMA i Örebro", "mma-i-orebro")]
    [InlineData("Träning & Hälsa", "traning-halsa")]
    public async Task UpdateAsync_ShouldGenerateCorrectSlug(string title, string expected)
    {
        var article = new Article { Id = 1, AuthorId = "u1" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);
        _repoMock.Setup(r => r.SlugExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        await _service.UpdateAsync(new ArticleViewModel { Id = 1, Title = title }, "u1", false);

        article.Slug.Should().Be(expected);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSanitizeHtmlContent()
    {
        var article = new Article { Id = 1, AuthorId = "u1" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);
        var model = new ArticleViewModel { Id = 1, Content = "<script>bad()</script><p>Good</p>", Title = "T" };

        await _service.UpdateAsync(model, "u1", false);

        article.Content.Should().NotContain("<script>");
        article.Content.Should().Contain("<p>Good</p>");
    }
    [Fact]
    public async Task ToggleLikeAsync_ShouldAddLike_WhenUserHasNotLiked()
    {
        // Arrange
        var articleId = 1;
        var userId = "user1";

        _repoMock.Setup(r => r.HasUserLikedArticleAsync(articleId, userId)).ReturnsAsync(false);
        _repoMock.Setup(r => r.GetLikesCountAsync(articleId)).ReturnsAsync(1);

        // Act
        var result = await _service.ToggleLikeAsync(articleId, userId);

        // Assert
        _repoMock.Verify(r => r.AddLikeAsync(articleId, userId), Times.Once);
        _repoMock.Verify(r => r.RemoveLikeAsync(articleId, userId), Times.Never);
        result.IsLiked.Should().BeTrue();
        result.LikesCount.Should().Be(1);
    }

    [Fact]
    public async Task ToggleLikeAsync_ShouldRemoveLike_WhenUserHasAlreadyLiked()
    {
        // Arrange
        var articleId = 1;
        var userId = "user1";

        _repoMock.Setup(r => r.HasUserLikedArticleAsync(articleId, userId)).ReturnsAsync(true);
        _repoMock.Setup(r => r.GetLikesCountAsync(articleId)).ReturnsAsync(0);

        // Act
        var result = await _service.ToggleLikeAsync(articleId, userId);

        // Assert
        _repoMock.Verify(r => r.RemoveLikeAsync(articleId, userId), Times.Once);
        _repoMock.Verify(r => r.AddLikeAsync(articleId, userId), Times.Never);
        result.IsLiked.Should().BeFalse();
        result.LikesCount.Should().Be(0);
    }

    [Fact]
    public async Task GetLatestAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetLatestAsync(5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetLatestAsync(5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMostPopularAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetMostPopularAsync(5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetMostPopularAsync(5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEditorChoiceAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetEditorChoiceAsync(5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetEditorChoiceAsync(5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetByCategoryAsync(1, 1, 10)).ReturnsAsync(new List<Article>());
        var result = await _service.GetByCategoryAsync(1, 1, 10);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetLatestByCategoryIdsAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetLatestByCategoryIdsAsync(It.IsAny<List<int>>(), 5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetLatestByCategoryIdsAsync(new List<int> { 1, 2 }, 5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetMostPopularByCategoryIdsAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetMostPopularByCategoryIdsAsync(It.IsAny<List<int>>(), 5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetMostPopularByCategoryIdsAsync(new List<int> { 1, 2 }, 5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetEditorChoiceByCategoryIdsAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetEditorChoiceByCategoryIdsAsync(It.IsAny<List<int>>(), 5)).ReturnsAsync(new List<Article>());
        var result = await _service.GetEditorChoiceByCategoryIdsAsync(new List<int> { 1, 2 }, 5);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBackendArticlesAsync_ShouldReturnList()
    {
        _repoMock.Setup(r => r.GetAllBackendArticlesAsync()).ReturnsAsync(new List<Article>());
        var result = await _service.GetBackendArticlesAsync("user", true);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetForEditAsync_ShouldReturnNull_WhenArticleNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Article)null);
        var result = await _service.GetForEditAsync(1, "user", true);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEditorModelAsync_ShouldReturnModel()
    {
        _repoMock.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());
        var result = await _service.GetEditorModelAsync();
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBySlugAsync_ShouldReturnNull_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetBySlugAsync("slug")).ReturnsAsync((Article)null);
        var result = await _service.GetBySlugAsync("slug");
        result.Should().BeNull();
    }

    [Fact]
    public async Task IncrementViewCountAsync_ShouldCallRepo()
    {
        _repoMock.Setup(r => r.IncrementViewCountAsync(1)).Returns(Task.CompletedTask).Verifiable();
        await _service.IncrementViewCountAsync(1);
        _repoMock.Verify(r => r.IncrementViewCountAsync(1), Times.Once);
    }

    [Fact]
    public async Task HasUserLikedArticleAsync_ShouldCallRepo()
    {
        _repoMock.Setup(r => r.HasUserLikedArticleAsync(1, "user")).ReturnsAsync(true);
        var result = await _service.HasUserLikedArticleAsync(1, "user");
        result.Should().BeTrue();
    }
}