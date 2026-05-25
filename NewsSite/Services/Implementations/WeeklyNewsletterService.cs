using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class WeeklyNewsletterService(
    ApplicationDbContext dbContext,
    IArticleService articleService,
    IEmailSender emailSender,
    ILogger<WeeklyNewsletterService> logger) : IWeeklyNewsletterService
{
    public async Task<int> SendWeeklyNewsletterAsync()
    {
        var preferences = await dbContext.NewsletterPreferences
            .Include(p => p.User)
            .Where(p => p.ReceiveNewsletter)
            .ToListAsync();

        var sentCount = 0;

        foreach (var preference in preferences)
        {
            var user = preference.User;

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
            {
                continue;
            }

            var categoryIds = ParseIntList(preference.SelectedCategoryIds);
            var authorIds = ParseStringList(preference.SelectedAuthIds);

            var articles = (await articleService.GetAllArticlesSortedByPreferencesAsync(
                categoryIds,
                authorIds,
                10)).ToList();

            if (articles.Count == 0)
            {
                logger.LogInformation("No matching articles found for newsletter recipient {Email}.", user.Email);
                continue;
            }

            var body = BuildHtmlBody(articles.Select(a => a.Title));

            try
            {
                await emailSender.SendEmailAsync(
                    user.Email,
                    "Your weekly newsletter",
                    body);

                sentCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send weekly newsletter to {Email}.", user.Email);
            }
        }

        logger.LogInformation("Weekly newsletter run completed. Sent {SentCount} emails out of {TotalRecipients} recipients.", sentCount, preferences.Count);
        return sentCount;
    }

    private static List<int> ParseIntList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var parsed) ? parsed : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
    }

    private static List<string> ParseStringList(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return [];
        }

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToList();
    }

    private static string BuildHtmlBody(IEnumerable<string> titles)
    {
        var builder = new StringBuilder();
        builder.Append("<h2>Your Weekly Newsletter</h2><ul>");

        foreach (var title in titles)
        {
            builder.Append("<li>")
                .Append(System.Net.WebUtility.HtmlEncode(title))
                .Append("</li>");
        }

        builder.Append("</ul>");
        return builder.ToString();
    }
}
