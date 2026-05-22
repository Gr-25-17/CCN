namespace NewsSite.Services.Interfaces;

public interface ISubscriptionReminderService
{
    Task<int> SendRenewalRemindersAsync(int daysBeforeExpiry = 7);
}
