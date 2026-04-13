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
    private readonly ArticleService _service;

    public ArticleServiceTests()
    {
        _repoMock = new Mock<IArticleRepository>();
        _service = new ArticleService(_repoMock.Object);
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
}