using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;
using Polly;
using Microsoft.Net.Http.Headers;

namespace NewsSite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            var lexiconConnection = builder.Configuration.GetConnectionString("LexiconConnection");

            if (!string.IsNullOrWhiteSpace(lexiconConnection))
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(lexiconConnection));
            }
            else
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddScoped<IArticleService, ArticleService>();
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddScoped<IUnsubscribeTokenService, UnsubscribeTokenService>();

            builder.Services.AddSingleton(_ =>
            {
                var storageConnectionString = builder.Configuration["AzureWebJobsStorage"];

                if (string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    throw new InvalidOperationException("Configuration value 'AzureWebJobsStorage' is missing.");
                }

                return new BlobServiceClient(storageConnectionString);
            });

            builder.Services.AddScoped<IBlobService, BlobService>();
            builder.Services.AddHttpClient();
            builder.Services.AddScoped<IImageOrchestrationService, ImageOrchestrationService>();
            builder.Services.AddControllers();
            builder.Services.AddHttpClient<IWeatherService, WeatherService>()
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            builder.Services.AddScoped<IWeatherService, WeatherService>();
            builder.Services.AddScoped<IGoldService, GoldService>();
            builder.Services.AddScoped<INewsletterService, NewsletterService>();
            builder.Services.AddScoped<INewsletterPreferenceRepository, NewsletterPreferenceRepository>();
            builder.Services.AddScoped<ILocalToSqlServerMigrationService, LocalToSqlServerMigrationService>();
            builder.Services.AddScoped<ISubscriptionAnalyticsService, SubscriptionAnalyticsService>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PremiumContent", policy =>
                    policy.RequireRole("Admin", "Editor", "Writer", "Subscriber"));

                options.AddPolicy("ManagementOnly", policy =>
                    policy.RequireRole("Admin", "Editor", "Writer"));
            });

            var app = builder.Build();

            var provider = new FileExtensionContentTypeProvider();
       
            provider.Mappings[".br"] = "application/octet-stream";
            provider.Mappings[".wasm"] = "application/wasm";
            provider.Mappings[".data"] = "application/octet-stream";
            provider.Mappings[".js"] = "application/javascript";

            var staticFileOptions = new StaticFileOptions
            {
                ContentTypeProvider = provider,
                OnPrepareResponse = ctx =>
                {
                    var path = ctx.Context.Request.Path.Value ?? "";

                    // Check if the actual file requested is a Brotli file
                    if (path.EndsWith(".br", StringComparison.OrdinalIgnoreCase))
                    {
                        // Crucial: Tell the browser it needs to decompress this via Brotli first
                        ctx.Context.Response.Headers[HeaderNames.ContentEncoding] = "br";

                        // If it's your framework file (e.g., Panchinko.framework.js.br)
                        if (path.Contains(".js.br", StringComparison.OrdinalIgnoreCase) ||
                            path.Contains(".framework", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Context.Response.Headers[HeaderNames.ContentType] = "application/javascript";
                        }
                        // If it's your data block (e.g., Panchinko.data.br)
                        else if (path.Contains(".data.br", StringComparison.OrdinalIgnoreCase))
                        {
                            ctx.Context.Response.Headers[HeaderNames.ContentType] = "application/octet-stream";
                        }
                    }
                }
            };

            if (args.Contains("--migrate-local-to-lexicon", StringComparer.OrdinalIgnoreCase))
            {
                using var migrationScope = app.Services.CreateScope();
                var migrationService = migrationScope.ServiceProvider.GetRequiredService<ILocalToSqlServerMigrationService>();
                var migratedRows = await migrationService.MigrateAsync();
                Console.WriteLine($"Local SQLite data migration completed. Total rows upserted: {migratedRows}.");
                return;
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles(staticFileOptions);
            app.UseRouting();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages()
               .WithStaticAssets();
            app.MapControllers();
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                if (context.Database.IsSqlite())
                {
                    await context.Database.MigrateAsync();
                }
                else
                {
                    await context.Database.EnsureCreatedAsync();
                }

                await DbInitializer.SeedRolesAndAdminAsync(services);
                await SeedData.InitializeAsync(services);
            }
            app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
