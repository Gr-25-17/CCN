using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentityCore<ApplicationUser>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddTransient<IEmailSender, EmailSender>();

        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IArticleArchiveService, ArticleArchiveService>();
        services.AddScoped<ISubscriptionReminderService, SubscriptionReminderService>();
        services.AddScoped<IWeeklyNewsletterService, WeeklyNewsletterService>();
    })
    .Build();

await host.RunAsync();
