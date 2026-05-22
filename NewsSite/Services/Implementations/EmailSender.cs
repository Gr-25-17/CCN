using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NewsSite.Services.Implementations
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var smtpPortValue = _config["EmailSettings:SmtpPort"];
                var smtpUser = _config["EmailSettings:SmtpUser"];
                var smtpPass = _config["EmailSettings:SmtpPass"];
                var senderEmail = _config["EmailSettings:SenderEmail"];
                var senderName = _config["EmailSettings:SenderName"];

                if (string.IsNullOrWhiteSpace(smtpServer))
                {
                    _logger.LogWarning("SMTP server is not configured (EmailSettings:SmtpServer).");
                    return;
                }

                if (!int.TryParse(smtpPortValue, out var smtpPort))
                {
                    smtpPort = 25;
                }

                using var client = new SmtpClient(smtpServer, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = smtpPort == 465 || smtpPort == 587 || smtpPort == 2525
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail ?? "noreply@localhost", senderName ?? "NewsSite"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", email);
                throw;
            }
        }
    }
}