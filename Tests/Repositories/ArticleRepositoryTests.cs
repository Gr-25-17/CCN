using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;
using FluentAssertions;
using Xunit;

namespace NewsSite.Tests.Repositories;

public class ArticleRepositoryTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
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
    public async Task ToggleLikeAsync_ShouldIncrementCount_OnNewLike()
    {
        using var context = CreateContext();
        var repo = new ArticleRepository(context);

        var result = await repo.ToggleLikeAsync(1, "user1");

        result.IsLiked.Should().BeTrue();
        result.LikesCount.Should().Be(1);
    }

   
}