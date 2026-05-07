using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NewsletterSender.Models;
using NewsletterSender.Services;
using System.Net;

namespace NewsletterSender.Functions;

public class SeedTestSubscribersFunction
{
    private readonly SubscriberRepository _subscriberRepository;
    private readonly ILogger<SeedTestSubscribersFunction> _logger;

    public SeedTestSubscribersFunction(SubscriberRepository subscriberRepository, ILoggerFactory loggerFactory)
    {
        _subscriberRepository = subscriberRepository;
        _logger = loggerFactory.CreateLogger<SeedTestSubscribersFunction>();
    }

    [Function("SeedTestSubscribers")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "newsletter/seed-test-subscribers")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        try
        {
            var list = new List<Subscriber>
            {
                new Subscriber { UserId = "testuser1", Email = "test1@local.test", FirstName = "Test1", PreferredCategoryIds = "1,2", IsActive = true },
                new Subscriber { UserId = "testuser2", Email = "test2@local.test", FirstName = "Test2", PreferredCategoryIds = "2", IsActive = true },
                new Subscriber { UserId = "testuser3", Email = "test3@local.test", FirstName = "Test3", PreferredCategoryIds = "1", IsActive = true },
                new Subscriber { UserId = "johan.liljeberg", Email = "johan.liljeberg@hotmail.com", FirstName = "Johan", PreferredCategoryIds = "1", IsActive = true }
            };

            foreach (var s in list)
            {
                await _subscriberRepository.UpsertSubscriberAsync(s);
            }

            await response.WriteStringAsync($"Seeded {list.Count} test subscribers.");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding test subscribers");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error: {ex.Message}");
            return response;
        }
    }
}
