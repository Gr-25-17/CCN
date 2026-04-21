using Microsoft.EntityFrameworkCore;
using NewsSite.Models.Entities;

namespace NewsSite.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureCreatedAsync();

        if (context.Articles.Any())
        {
            return;
        }

        if (!context.Categories.Any())
        {
            var categories = new Category[]
            {
                new Category { Name = "Sweden" },
                new Category { Name = "World" },
                new Category { Name = "Sport" },
                new Category { Name = "Economy" },
                new Category { Name = "Weather" }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        var sweden = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sweden");
        var world = await context.Categories.FirstOrDefaultAsync(c => c.Name == "World");
        var sport = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Sport");
        var economy = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Economy");
        var weather = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Weather");

        var articles = new Article[]
        {
            new Article
            {
                Title = "Sweden's Economy Shows Strong Growth",
                Summary = "The Swedish economy grew by 2.5% this quarter, exceeding expectations.",
                Content = "Detailed article content about Sweden's economic growth...",
                Slug = "sweden-economy-growth",
                ImageUrl = "https://images.pexels.com/photos/6770775/pexels-photo-6770775.jpeg",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsReadyForPublish = true,
                IsArchived = false,
                IsDeleted = false,
                IsEditorsChoice = true,
                CategoryId = sweden?.Id ?? 1
            },
            new Article
            {
                Title = "Global Climate Summit Reaches Historic Agreement",
                Summary = "World leaders agree on new climate targets at the summit in Paris.",
                Content = "Detailed article content about the climate summit...",
                Slug = "global-climate-summit",
                ImageUrl = "https://images.pexels.com/photos/2990653/pexels-photo-2990653.jpeg",
                CreatedAt = DateTime.Now.AddDays(-2),
                IsReadyForPublish = true,
                IsArchived = false,
                IsDeleted = false,
                IsEditorsChoice = false,
                CategoryId = world?.Id ?? 2
            },
            new Article
            {
                Title = "Local Football Team Wins Championship",
                Summary = "A thrilling final match ends with a 3-2 victory for the home team.",
                Content = "Detailed article content about the football match...",
                Slug = "football-championship-win",
                ImageUrl = "https://images.pexels.com/photos/36213442/pexels-photo-36213442.jpeg",
                CreatedAt = DateTime.Now.AddDays(-3),
                IsReadyForPublish = true,
                IsArchived = false,
                IsDeleted = false,
                IsEditorsChoice = false,
                CategoryId = sport?.Id ?? 3
            },
            new Article
            {
                Title = "New Technology Trends for 2024",
                Summary = "AI and sustainable tech lead the way in this year's innovations.",
                Content = "Detailed article content about tech trends...",
                Slug = "tech-trends-2024",
                ImageUrl = "https://images.pexels.com/photos/7562023/pexels-photo-7562023.jpeg",
                CreatedAt = DateTime.Now.AddDays(-4),
                IsReadyForPublish = true,
                IsArchived = false,
                IsDeleted = false,
                IsEditorsChoice = true,
                CategoryId = world?.Id ?? 2
            },
            new Article
            {
                Title = "Weather Warning: Storm Approaching West Coast",
                Summary = "Residents advised to prepare for heavy winds and rain this weekend.",
                Content = "Detailed article content about the weather warning...",
                Slug = "storm-warning-west-coast",
                ImageUrl = "https://images.pexels.com/photos/2260967/pexels-photo-2260967.jpeg",
                CreatedAt = DateTime.Now.AddDays(-1),
                IsReadyForPublish = true,
                IsArchived = false,
                IsDeleted = false,
                IsEditorsChoice = false,
                CategoryId = weather?.Id ?? 5
            }
        };

        await context.Articles.AddRangeAsync(articles);
        await context.SaveChangesAsync();
    }
}