using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace NewsSite.Services.Implementations
{
    public class EmailSender(IConfiguration config) : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpServer = config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(config["EmailSettings:SmtpPort"]!);
            var smtpUser = config["EmailSettings:SmtpUser"];
            var smtpPass = config["EmailSettings:SmtpPass"];
            var senderEmail = config["EmailSettings:SenderEmail"];
            var senderName = config["EmailSettings:SenderName"];

            var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}