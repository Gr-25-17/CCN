

using FluentAssertions;
using Moq;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;

namespace Tests.Services
{
    public class ArticleServiceTests
    {
        private readonly ArticleService _articleService;
        private readonly Mock<IArticleRepository> _articleRepoMock;
    
        public ArticleServiceTests()
        {
            _articleRepoMock = new Mock<IArticleRepository>();
            _articleService = new ArticleService(_articleRepoMock.Object);
        }

        [Fact]
        public async Task UpdateAsync_ShouldSanitizeContent_WhenHtmlIsProvided()
        {
            // Arrange
            var article = new Article
            {
                Id = 1,
                IsLocked = false
            };
            var model = new ArticleViewModel
            {
                Id = 1,
                Title = "Test Article",
                Content = "<script>alert('XSS');</script><p>Valid content.</p>",
            };
            _articleRepoMock.Setup(r=>r.GetByIdAsync(1)).ReturnsAsync(article);

            await _articleService.UpdateAsync(model, "author-id", true);

            article.Content.Should().NotContain("<script>");
            article.Content.Should().Contain("<p>Valid</p>");
        }
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenArticleIsLocked()
        {
            var article = new Article { Id = 1, IsLocked = true };
            var model = new ArticleViewModel { Id = 1 };

            _articleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);

            var result = await _articleService.UpdateAsync(model, "author-id", true);

            result.Should().BeFalse();
            _articleRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Article>()), Times.Never);

        }

        [Fact]
        public async Task UpdateAsync_ShouldGenerateCorrectSlug_WithSwedishCharacters()
        {
            var article = new Article { Id = 1, IsLocked = false };
            var model = new ArticleViewModel { Id = 1, Title = "Här är en artikel" };

            _articleRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(article);

            await _articleService.UpdateAsync(model, "author-id", true);
            article.Slug.Should().Be("har-ar-en-artikel");
        }

    }
}
