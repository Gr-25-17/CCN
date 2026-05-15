using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsSite.Services.Interfaces;

namespace NewsSite.Functions;

public class WeeklyNewsletterFunction(
    IWeeklyNewsletterService weeklyNewsletterService,
    ILogger<WeeklyNewsletterFunction> logger)
{
    [Function(nameof(WeeklyNewsletterFunction))]
    public async Task Run([TimerTrigger("0 0 8 * * 1")] TimerInfo timer)
    {
        var sentCount = await weeklyNewsletterService.SendWeeklyNewsletterAsync();
        logger.LogInformation("Sent {Count} weekly newsletters.", sentCount);
    }
}
