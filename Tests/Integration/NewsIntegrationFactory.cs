using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NewsSite.Data;
using NewsSite.Models.Entities;
using System.Data.Common; // LÄGG TILL DENNA FÖR ATT KUNNA TA BORT SQLITE-ANSLUTNINGEN

namespace NewsSite.Tests.Integration;

public class NewsIntegrationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "IntegrationTestDb_" + Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // 1. Ta bort DbContext konfigurationen
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // 2. DENNA ÄR NY: Ta bort SQLite:s databas-anslutning helt från systemet
            var dbConnectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));
            if (dbConnectionDescriptor != null) services.Remove(dbConnectionDescriptor);

            // Ta bort alla återstående service-registreringar som kan komma från SQLite-provider
            // (det förekommer ibland registeringar där ServiceType/ImplementationType innehåller "Sqlite")
            var sqliteDescriptors = services.Where(d =>
                (d.ServiceType?.FullName?.Contains("Sqlite") ?? false) ||
                (d.ImplementationType?.FullName?.Contains("Sqlite") ?? false) ||
                (d.ImplementationInstance?.GetType().FullName?.Contains("Sqlite") ?? false)
            ).ToList();
            foreach (var d in sqliteDescriptors) services.Remove(d);

            // 3. Nu är fältet helt tomt, vi kan säkert lägga till InMemory!
            // Use a dedicated internal service provider for the InMemory provider to avoid conflicts
            // when the original service collection still contains registrations from Sqlite.
            var inMemoryServiceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
                options.UseInternalServiceProvider(inMemoryServiceProvider);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Säkerställ ren databas
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seeda in data för testet
            var cat = new Category { Name = "Tech", Id = 1 };
            context.Categories.Add(cat);

            context.Articles.Add(new Article
            {
                Title = "Premium Story",
                Slug = "premium-story",
                Content = "<p>Första stycket.</p><p>Andra stycket.</p><p>Tredje stycket.</p>",
                IsPremium = true,
                IsReadyForPublish = true,
                CategoryId = 1,
                ViewsCount = 10
            });
            context.SaveChanges();
        });
    }
}