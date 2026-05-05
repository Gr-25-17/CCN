using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NewsletterSender.Services;

namespace NewsletterSender.Functions;

public class WeeklyNewsletterTimer
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;
    private readonly SubscriberRepository _subscriberRepository;
    private readonly NewsletterBuilder _newsletterBuilder;
    private readonly EmailSender _emailSender;
    private readonly DeliveryLogger _deliveryLogger;

    public WeeklyNewsletterTimer(
        ILoggerFactory loggerFactory,
        IConfiguration configuration,
        SubscriberRepository subscriberRepository,
        NewsletterBuilder newsletterBuilder,
        EmailSender emailSender,
        DeliveryLogger deliveryLogger)
    {
        _logger = loggerFactory.CreateLogger<WeeklyNewsletterTimer>();
        _configuration = configuration;
        _subscriberRepository = subscriberRepository;
        _newsletterBuilder = newsletterBuilder;
        _emailSender = emailSender;
        _deliveryLogger = deliveryLogger;
    }

    [Function("WeeklyNewsletterSender")]
    public async Task Run([TimerTrigger("0 0 8 * * 1")] TimerInfo myTimer)
    {
        _logger.LogInformation($"Weekly Newsletter Sender started at: {DateTime.UtcNow}");

        try
        {
            // Load all active subscribers
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();
            _logger.LogInformation($"Loaded {subscribers.Count} active subscribers");

            if (!subscribers.Any())
            {
                _logger.LogInformation("No active subscribers found. Exiting.");
                return;
            }

            var newsletterId = Guid.NewGuid().ToString();
            var sentAt = DateTime.UtcNow;

            // Batch sending in groups of 50 to avoid overwhelming the provider
            int batchSize = 50;
            for (int i = 0; i < subscribers.Count; i += batchSize)
            {
                var batch = subscribers.Skip(i).Take(batchSize).ToList();
                _logger.LogInformation($"Processing batch {i / batchSize + 1} ({batch.Count} subscribers)");

                // Send newsletters for this batch
                foreach (var subscriber in batch)
                {
                    try
                    {
                        // Build personalized newsletter content
                        var newsletterContent = await _newsletterBuilder.BuildPersonalizedNewsletterAsync(subscriber);

                        // Send email
                        await _emailSender.SendNewsletterAsync(
                            subscriber.Email,
                            subscriber.FirstName,
                            newsletterContent.HtmlBody,
                            newsletterContent.Subject);

                        // Log successful delivery
                        await _deliveryLogger.LogDeliveryAsync(
                            newsletterId,
                            subscriber.Email,
                            subscriber.UserId,
                            sentAt,
                            "Sent",
                            null);

                        _logger.LogInformation($"Newsletter sent to {subscriber.Email}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to send newsletter to {subscriber.Email}: {ex.Message}");

                        // Log failed delivery
                        await _deliveryLogger.LogDeliveryAsync(
                            newsletterId,
                            subscriber.Email,
                            subscriber.UserId,
                            sentAt,
                            "Failed",
                            ex.Message);
                    }

                    // Small delay to throttle sending
                    await Task.Delay(500);
                }
            }

            // Update LastSentAt for all subscribers
            await _subscriberRepository.UpdateLastSentAtAsync(subscribers.Select(s => s.UserId).ToList(), sentAt);

            _logger.LogInformation($"Weekly Newsletter Sender completed at: {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error in WeeklyNewsletterTimer: {ex}");
            throw;
        }
    }
}
