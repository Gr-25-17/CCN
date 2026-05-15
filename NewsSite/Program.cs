using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Implementations;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;
using Polly;

namespace NewsSite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(connectionString));
            }
            else
            {
                var azureSqlConnection = builder.Configuration.GetConnectionString("AzureSqlConnection");

                if (string.IsNullOrEmpty(azureSqlConnection))
                {
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(connectionString));
                }
                else
                {
                    builder.Services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(azureSqlConnection));
                }
            }

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
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

            builder.Services.AddHttpClient<IWeatherService, WeatherService>()
                .AddTransientHttpErrorPolicy(policy =>
                    policy.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            builder.Services.AddScoped<IWeatherService, WeatherService>();
            builder.Services.AddScoped<IGoldService, GoldService>();
            builder.Services.AddScoped<INewsletterService, NewsletterService>();
            builder.Services.AddScoped<INewsletterPreferenceRepository, NewsletterPreferenceRepository>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("PremiumContent", policy =>
                    policy.RequireRole("Admin", "Editor", "Writer", "Subscriber"));

                options.AddPolicy("ManagementOnly", policy =>
                    policy.RequireRole("Admin", "Editor", "Writer"));
            });

            var app = builder.Build();

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

                context.Database.Migrate();

                await DbInitializer.SeedRolesAndAdminAsync(services);
                await SeedData.InitializeAsync(services);
            }

            await app.RunAsync();
        }
    }
}
