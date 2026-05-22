using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Data;
using System.Net;

namespace Tests.Integration
{
    public class PremiumWallAndPersistenceTests(NewsIntegrationFactory factory) : IClassFixture<NewsIntegrationFactory>
    {
        //[Fact]
        //public async Task Get_PremiumArticle_AsGuest_ShouldTruncateContentAndIncrementViews()
        //{
        //    // Arrange
        //    string slug = "premium-test-slug";
        //    factory.SeedArticle(slug, isPremium: true);
        //    var client = factory.CreateClient();

        //    // Act
        //    // Vi skickar slug som query-parameter eftersom din controller accepterar det
        //    var response = await client.GetAsync($"/Articles/Details?slug={slug}");
        //    var html = await response.Content.ReadAsStringAsync();

        //    // Assert
        //    response.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Verifiera Truncation (Premium-logik i ArticlesController.cs)
        //    // Din kod kör: paragraphs.Take(2)
        //    html.Should().Contain("<p>Stycke 1.</p>");
        //    html.Should().Contain("<p>Stycke 2.</p>");
        //    html.Should().NotContain("<p>Stycke 3.</p>");

        //    // Verifiera Databaspersistens (ViewsCount)
        //    // Din ArticleRepository använder ExecuteUpdateAsync för detta
        //    using var scope = factory.Services.CreateScope();
        //    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        //    var articleInDb = await db.Articles
        //        .AsNoTracking()
        //        .SingleAsync(a => a.Slug == slug);

        //    // Den startade på 10, IncrementViewCountAsync körs i Details-metoden
        //    articleInDb.ViewsCount.Should().Be(11);
        //}
    }
}