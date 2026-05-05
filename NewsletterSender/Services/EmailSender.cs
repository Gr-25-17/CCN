using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NewsletterSender.Services;

/// <summary>
/// Sends emails using SendGrid with batching and retry support.
/// </summary>
public class EmailSender
{
    private readonly SendGridClient _sendGridClient;
    private readonly ILogger _logger;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailSender(IConfiguration config, ILoggerFactory loggerFactory)
    {
        var sendGridApiKey = config["SendGridApiKey"];
        if (string.IsNullOrEmpty(sendGridApiKey))
        {
            throw new InvalidOperationException("SendGridApiKey configuration is missing");
        }

        _sendGridClient = new SendGridClient(sendGridApiKey);
        _logger = loggerFactory.CreateLogger<EmailSender>();
        _fromEmail = config["NewsletterFromEmail"] ?? "noreply@newssite.com";
        _fromName = config["NewsletterFromName"] ?? "NewsletterSender";
    }

    /// <summary>
    /// Sends a newsletter email to a subscriber.
    /// </summary>
    public async Task SendNewsletterAsync(string toEmail, string toName, string htmlContent, string subject)
    {
        try
        {
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail, toName);

            var msg = new SendGridMessage()
            {
                From = from,
                Subject = subject,
                HtmlContent = htmlContent,
                PlainTextContent = StripHtmlTags(htmlContent)
            };

            msg.AddTo(to);

            // Set reply-to if configured
            var replyTo = new EmailAddress(
                _fromEmail,
                _fromName);
            msg.SetReplyTo(replyTo);

            // Send with retry logic
            var response = await SendWithRetryAsync(msg, maxRetries: 3);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError($"Failed to send email to {toEmail}: {response.StatusCode} - {body}");
                throw new InvalidOperationException($"SendGrid returned {response.StatusCode}: {body}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending newsletter to {toEmail}: {ex.Message}");
            throw;
        }
    }

    private async Task<Response> SendWithRetryAsync(SendGridMessage msg, int maxRetries = 3)
    {
        int attempt = 0;
        while (attempt < maxRetries)
        {
            try
            {
                attempt++;
                var response = await _sendGridClient.SendEmailAsync(msg);

                // Return on success or permanent error
                if (response.IsSuccessStatusCode || (int)response.StatusCode >= 400)
                {
                    return response;
                }

                // Retry on transient errors (429, 5xx)
                if (attempt < maxRetries && ((int)response.StatusCode == 429 || (int)response.StatusCode >= 500))
                {
                    var delayMs = (int)Math.Pow(2, attempt) * 1000; // Exponential backoff
                    _logger.LogWarning($"SendGrid rate limited or server error. Retrying in {delayMs}ms...");
                    await Task.Delay(delayMs);
                    continue;
                }

                return response;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning($"Attempt {attempt} failed: {ex.Message}. Retrying...");
                await Task.Delay((int)Math.Pow(2, attempt) * 1000);
            }
        }

        throw new InvalidOperationException($"SendGrid request failed after {maxRetries} retries");
    }

    private static string StripHtmlTags(string html)
    {
        // Simple HTML tag stripping for plain text version
        return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", "");
    }
}
