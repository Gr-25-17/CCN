using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class WeeklyNewsletterService(
    ApplicationDbContext dbContext,
    IArticleService articleService,
    IEmailSender emailSender) : IWeeklyNewsletterService
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

            var articles = (await articleService.GetAllArticlesSortedByPreferencesAsync(
                preference.SelectedCategoryIds ?? string.Empty,
                preference.SelectedAuthIds ?? string.Empty,
                10)).ToList();

            if (articles.Count == 0)
            {
                continue;
            }

            var body = BuildHtmlBody(articles.Select(a => a.Title));

            await emailSender.SendEmailAsync(
                user.Email,
                "Your weekly newsletter",
                body);

            sentCount++;
        }

        return sentCount;
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
