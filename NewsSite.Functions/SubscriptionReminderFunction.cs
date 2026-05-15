using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsSite.Services.Interfaces;

namespace NewsSite.Functions;

public class SubscriptionReminderFunction(
    ISubscriptionReminderService reminderService,
    ILogger<SubscriptionReminderFunction> logger)
{
    [Function(nameof(SubscriptionReminderFunction))]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        var sentCount = await reminderService.SendRenewalRemindersAsync();
        logger.LogInformation("Sent {Count} subscription renewal reminders.", sentCount);
    }
}
