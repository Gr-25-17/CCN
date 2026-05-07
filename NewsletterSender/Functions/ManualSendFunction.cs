using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NewsletterSender.Services;
using System.Net;

namespace NewsletterSender.Functions;

public class ManualSendFunction
{
    private readonly SubscriberRepository _subscriberRepository;
    private readonly NewsletterBuilder _newsletterBuilder;
    private readonly EmailSender _emailSender;
    private readonly DeliveryLogger _deliveryLogger;
    private readonly ILogger<ManualSendFunction> _logger;

    public ManualSendFunction(SubscriberRepository subscriberRepository,
        NewsletterBuilder newsletterBuilder,
        EmailSender emailSender,
        DeliveryLogger deliveryLogger,
        ILoggerFactory loggerFactory)
    {
        _subscriberRepository = subscriberRepository;
        _newsletterBuilder = newsletterBuilder;
        _emailSender = emailSender;
        _deliveryLogger = deliveryLogger;
        _logger = loggerFactory.CreateLogger<ManualSendFunction>();
    }

    [Function("ManualSendNewsletter")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "newsletter/send-manual")] HttpRequestData req)
    {
        _logger.LogInformation("Manual send triggered");

        var response = req.CreateResponse(HttpStatusCode.OK);

        try
        {
            var subscribers = await _subscriberRepository.GetActiveSubscribersAsync();
            if (!subscribers.Any())
            {
                await response.WriteStringAsync("No active subscribers");
                return response;
            }

            int sent = 0, failed = 0;
            var newsletterId = Guid.NewGuid().ToString();
            var sentAt = DateTime.UtcNow;

            foreach (var subscriber in subscribers.Take(5)) // test with first 5
            {
                try
                {
                    var content = await _newsletterBuilder.BuildPersonalizedNewsletterAsync(subscriber);
                    await _emailSender.SendNewsletterAsync(subscriber.Email, subscriber.FirstName, content.HtmlBody, content.Subject);
                    await _deliveryLogger.LogDeliveryAsync(newsletterId, subscriber.Email, subscriber.UserId, sentAt, "Sent", null);
                    sent++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending to {Email}", subscriber.Email);
                    await _deliveryLogger.LogDeliveryAsync(newsletterId, subscriber.Email, subscriber.UserId, sentAt, "Failed", ex.Message);
                    failed++;
                }
            }

            await response.WriteStringAsync($"Manual send completed. Sent: {sent}, Failed: {failed}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Manual send error");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }
}
