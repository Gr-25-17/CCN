using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;
using FluentAssertions;
using Xunit;

namespace Tests.Repositories;

public class CategoryRepositoryTests
{
    private ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategoriesOrderedById()
    {
        // Arrange
        using var context = CreateContext();
        context.Categories.AddRange(
            new Category { Id = 2, Name = "Sport" },
            new Category { Id = 1, Name = "Sweden" },
            new Category { Id = 3, Name = "Economy" }
        );
        await context.SaveChangesAsync();

        var repo = new CategoryRepository(context);

        // Act
        var result = (await repo.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Id.Should().Be(1);
        result[0].Name.Should().Be("Sweden");
        result[1].Id.Should().Be(2);
        result[1].Name.Should().Be("Sport");
        result[2].Id.Should().Be(3);
        result[2].Name.Should().Be("Economy");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        using var context = CreateContext();
        var repo = new CategoryRepository(context);

        // Act
        var result = await repo.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }
}