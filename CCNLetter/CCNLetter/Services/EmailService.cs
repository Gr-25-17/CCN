using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> SendEmailAsync(string email, string subject, string content)
        {
            try
            {
                // VALIDATE ALL REQUIRED FIELDS
                if (string.IsNullOrWhiteSpace(email))
                    return "Error: Recipient email is empty";

                if (string.IsNullOrWhiteSpace(subject))
                    return "Error: Subject is empty";

                if (string.IsNullOrWhiteSpace(content))
                    return "Error: Email content is empty";

                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderName = _configuration["EmailSettings:SenderName"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = _configuration["EmailSettings:SmtpPort"];
                var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
                var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
                // CHECK CONFIGURATION VALUES
                if (string.IsNullOrWhiteSpace(senderEmail))
                    return "Error: SenderEmail is not configured in appsettings.json";

                if (string.IsNullOrWhiteSpace(senderName))
                    return "Error: SenderName is not configured in appsettings.json";

                if (string.IsNullOrWhiteSpace(smtpServer))
                    return "Error: SmtpServer is not configured";

                if (string.IsNullOrWhiteSpace(smtpPort))
                    return "Error: SmtpPort is not configured";

                if (string.IsNullOrWhiteSpace(smtpUsername))
                    return "Error: SmtpUsername is not configured";

                if (string.IsNullOrWhiteSpace(smtpPassword))
                    return "Error: SmtpPassword is not configured";

                // Create the email message
                var message = new MimeMessage();
                message.Sender = MailboxAddress.Parse(senderEmail);
                message.Sender.Name = senderName;
                message.To.Add(MailboxAddress.Parse(email));
                message.From.Add(message.Sender);
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = content };

                // Send the email
                using (var emailClient = new SmtpClient())
                {
                    await emailClient.ConnectAsync(smtpServer, int.Parse(smtpPort), true);
                    emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                    await emailClient.AuthenticateAsync(smtpUsername, smtpPassword);
                    await emailClient.SendAsync(message);
                    await emailClient.DisconnectAsync(true);
                }

                return string.Empty; // Success
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
