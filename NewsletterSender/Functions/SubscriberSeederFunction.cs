using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterSender.Services;
using NewsSite.Data;

namespace NewsletterSender.Functions;

/// <summary>
/// Admin function to seed subscribers from NewsSite database to Azure Table Storage.
/// Triggered manually or on demand.
/// </summary>
public class SubscriberSeederFunction
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly SubscriberSeeder _seeder;

    public SubscriberSeederFunction(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        SubscriberSeeder seeder)
    {
        _logger = loggerFactory.CreateLogger<SubscriberSeederFunction>();
        _configuration = configuration;
        _seeder = seeder;
    }

    [Function("SeedSubscribers")]
    public async Task Run([TimerTrigger("0 0 2 * * 0")] TimerInfo myTimer)
    {
        // This function runs weekly (Sundays at 02:00 UTC) to sync subscribers
        // Can also be triggered manually via Azure Functions Portal or CLI
        _logger.LogInformation($"Subscriber Seeder started at: {DateTime.UtcNow}");

        try
        {
            await _seeder.SeedSubscribersAsync();
            _logger.LogInformation("Subscriber seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SubscriberSeederFunction: {ex.Message}");
            throw;
        }
    }
}
