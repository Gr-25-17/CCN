using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NewsSite.Data;
using NewsSite.Models.Entities;
using System.Data.Common;

namespace NewsSite.Tests.Integration
{
    public class NewsIntegrationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use the Development environment so the app config registers the Sqlite provider
            // This prevents SqlServer from being registered alongside the in-memory Sqlite used by tests.
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Ta bort befintliga DB-registreringar och eventuella provider-specifika registreringar
                // (kan ha registrerats i Program.cs). Vi tar bort alla DbContextOptions och DbConnection
                // plus tjänster som refererar till SqlServer/Sqlite för att undvika flera providers i samma provider.
                services.RemoveAll(typeof(DbContextOptions<ApplicationDbContext>));
                services.RemoveAll(typeof(DbConnection));

                var providerDescriptors = services.Where(d =>
                    (d.ServiceType != null && (d.ServiceType.FullName?.Contains("SqlServer") == true || d.ServiceType.FullName?.Contains("Sqlite") == true)) ||
                    (d.ImplementationType != null && (d.ImplementationType.FullName?.Contains("SqlServer") == true || d.ImplementationType.FullName?.Contains("Sqlite") == true))
                ).ToList();
                foreach (var pd in providerDescriptors)
                {
                    services.Remove(pd);
                }

                // Skapa och öppna anslutningen manuellt
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });

                // VIKTIGT: Skapa tabellerna NU, innan resten av appen startar
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Database.EnsureCreated();
            });
        }

        public void SeedArticle(string slug, bool isPremium)
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Vi lägger till en kategori först
            var category = new Category { Name = "TestCategory" };
            context.Categories.Add(category);
            context.SaveChanges();

            context.Articles.Add(new Article
            {
                Title = "Integration Test Article",
                Slug = slug,
                Summary = "Summary content",
                Content = "<p>Stycke 1.</p><p>Stycke 2.</p><p>Stycke 3.</p>",
                IsPremium = isPremium,
                IsReadyForPublish = true,
                CategoryId = category.Id,
                AuthorName = "Test Author",
                CreatedAt = DateTime.UtcNow,
                ViewsCount = 10
            });
            context.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}