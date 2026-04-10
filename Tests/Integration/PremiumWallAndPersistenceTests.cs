using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Data;
using System.Net;
using Xunit;

namespace NewsSite.Tests.Integration;

public class PremiumWallAndPersistenceTests(NewsIntegrationFactory factory) : IClassFixture<NewsIntegrationFactory>
{
    [Fact]
    public async Task Get_PremiumArticle_AsGuest_ShouldTruncateContentAndIncrementViews()
    {
        var client = factory.CreateClient();

        // LÖSNING 2: Använd query-string för att 100% garantera att controllern hittar ordet "premium-story" oavsett routing
        var url = "/Articles/Details?slug=premium-story";

        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();

        // LÖSNING 3: Bättre felhantering. Om denna fallerar nu, får du en tydlig text om varför (t.ex. 404 eller 500)
        response.StatusCode.Should().Be(HttpStatusCode.OK, "Sidan borde ladda korrekt. Kolla HTML-felet: " + html);

        // Verifiera att texten är klippt
        html.Should().Contain("<p>Första stycket.</p>");
        html.Should().Contain("<p>Andra stycket.</p>");
        html.Should().NotContain("<p>Tredje stycket.</p>");

        // Verifiera att ViewsCount har uppdaterats i databasen
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // LÖSNING 4: AsNoTracking tvingar Entity Framework att läsa från databasen istället för ett gammalt sparat minne
        var article = db.Articles.AsNoTracking().Single(a => a.Slug == "premium-story");

        article.ViewsCount.Should().Be(11);
    }
}