using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;
using FluentAssertions;

namespace Tests.Repositories;

public class ArticleRepositoryTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    /*[Fact]
    public async Task GetBySlugAsync_ShouldOnlyReturnPublishedArticles()
    {
        using var context = CreateContext();

        // Skapa en kategori för att uppfylla Foreign Key-kravet
        var category = new Category { Name = "General" };
        context.Categories.Add(category);
        await context.SaveChangesAsync();

        context.Articles.AddRange(
            new Article
            {
                Slug = "test",
                IsReadyForPublish = false,
                CategoryId = category.Id,
                Title = "Opublicerad"
            },
            new Article
            {
                Slug = "ready",
                IsReadyForPublish = true,
                CategoryId = category.Id,
                Title = "Publicerad"
            }
        );
        await context.SaveChangesAsync();

        var repo = new ArticleRepository(context);

        var result = await repo.GetBySlugAsync("test");
        var resultReady = await repo.GetBySlugAsync("ready");

        result.Should().BeNull();
        resultReady.Should().NotBeNull();
        resultReady!.Slug.Should().Be("ready");
    }

    [Fact]
    public async Task LikeMethods_ShouldModifyDatabaseCorrectly()
    {
        using var context = CreateContext();
        var repo = new ArticleRepository(context);
        var articleId = 1;
        var userId = "user1";

        // Testar Add
        await repo.AddLikeAsync(articleId, userId);
        var exists = await repo.HasUserLikedArticleAsync(articleId, userId);
        var count = await repo.GetLikesCountAsync(articleId);

        exists.Should().BeTrue();
        count.Should().Be(1);

        // Testar Remove
        await repo.RemoveLikeAsync(articleId, userId);
        var existsAfter = await repo.HasUserLikedArticleAsync(articleId, userId);
        var countAfter = await repo.GetLikesCountAsync(articleId);

        existsAfter.Should().BeFalse();
        countAfter.Should().Be(0);
    }*/


}