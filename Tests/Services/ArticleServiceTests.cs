using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;
using System.Security.Claims;
using Xunit;

namespace NewsSite.Tests.Services;

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
}