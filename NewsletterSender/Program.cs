using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NewsSite.Data;
using NewsletterSender.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Add HTTP client for article service calls
        services.AddHttpClient<ArticleServiceClient>();

        // Add Entity Framework (for seeding from NewsSite database)
        var connectionString = context.Configuration["NewsSiteDbConnection"] ?? "Server=.;Database=NewsSiteDb;Integrated Security=true;";
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Add services
        services.AddSingleton<SubscriberRepository>();
        services.AddSingleton<NewsletterBuilder>();
        services.AddSingleton<EmailSender>();
        services.AddSingleton<DeliveryLogger>();
        services.AddScoped<SubscriberSeeder>();

        // Add logging
        services.AddLogging();
    })
    .Build();

host.Run();
