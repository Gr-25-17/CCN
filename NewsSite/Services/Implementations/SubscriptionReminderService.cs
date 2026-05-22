using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class SubscriptionReminderService(
    ApplicationDbContext dbContext,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender) : ISubscriptionReminderService
{
    public async Task<int> SendRenewalRemindersAsync(int daysBeforeExpiry = 7)
    {
        if (daysBeforeExpiry <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(daysBeforeExpiry));
        }

        var targetDate = DateTime.UtcNow.Date.AddDays(daysBeforeExpiry);

        var subscriptions = await dbContext.Subscriptions
            .Include(s => s.User)
            .Where(s =>
                s.PaymentComplete &&
                s.RenewalReminderSentAt == null &&
                s.EndDate.Date == targetDate)
            .ToListAsync();

        var sentCount = 0;

        foreach (var subscription in subscriptions)
        {
            var user = subscription.User;

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                continue;
            }

            await emailSender.SendEmailAsync(
                user.Email,
                "Your subscription expires soon",
                "Your subscription will expire in 7 days. Please renew to keep access to premium articles.");

            subscription.RenewalReminderSentAt = DateTime.UtcNow;
            sentCount++;
        }

        if (sentCount > 0)
        {
            await dbContext.SaveChangesAsync();
        }

        return sentCount;
    }
}
