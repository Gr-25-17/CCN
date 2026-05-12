using CCNLetter.Models;
using CCNLetter.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CCNLetter.Controller
{
    internal class NewsLetterFunction
    {
        private readonly IEmailService _emailService;
        private readonly INewsletterContentService _newsletterContentService;
        private readonly ILogger<NewsLetterFunction> _logger;

        public NewsLetterFunction(
            IEmailService emailService,
            INewsletterContentService newsletterContentService,
            ILogger<NewsLetterFunction> logger)
        {
            _emailService = emailService;
            _newsletterContentService = newsletterContentService;
            _logger = logger;
        }

        // Runs every 5 seconds for testing
        [Function("SendNewsletter")]
        //public async Task Run([TimerTrigger("0 0 8 * * 1")] TimerInfo myTimer)
            public async Task Run([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"Newsletter test started at: {DateTime.Now}");

            try
            {
                // Hardcoded test/Replace with  real articles)
                var articles = await _newsletterContentService.GetFeaturedArticlesAsync(5);

                _logger.LogInformation($"Found {articles.Count} test articles for newsletter");

                // Generate newsletter HTML
                var newsletterTitle = $"CCN Veckans Nyheter - {DateTime.Now:yyyy-MM-dd}";
                var htmlContent = await _newsletterContentService.GenerateNewsletterHtmlAsync(articles, newsletterTitle);

                // Send to my email for testing
                string testEmail = "johan.liljeberg90@gmail.com";
                string subject = newsletterTitle;

                var result = await _emailService.SendEmailAsync(testEmail, subject, htmlContent);

                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogInformation($" Newsletter sent successfully to {testEmail}");
                    foreach (var article in articles)
                    {
                        _logger.LogInformation($"Included: {article.Title}");
                    }
                }
                else
                {
                    _logger.LogError($" Email sending failed: {result}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Newsletter function failed");
            }
        }
    }
}