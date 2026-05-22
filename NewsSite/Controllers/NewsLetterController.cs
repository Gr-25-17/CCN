using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewsSite.Repositories.Implementations;
using NewsSite.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/newsletter")]
    public class NewsLetterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly INewsletterService _newsletterService;
        private readonly IArticleService _articleService;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplateBuilder _templateBuilder;

        public NewsLetterController(
            IConfiguration configuration,
            INewsletterService newsletterService,
            IArticleService articleService,
            IEmailSender emailSender,
            EmailTemplateBuilder templateBuilder)
        {
            _configuration = configuration;
            _newsletterService = newsletterService;
            _articleService = articleService;
            _emailSender = emailSender;
            _templateBuilder = templateBuilder;
        }

        [HttpPost("send-weekly")]
        public async Task<IActionResult> SendWeeklyNewsletter()
        {
            var apiKey = Request.Headers["X-API-Key"].ToString();
            var expectedKey = _configuration["NewsletterSettings:ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey != expectedKey)
            {
                return Unauthorized("Invalid API key");
            }

            try
            {          
                var subscribers = await _newsletterService.GetWeeklySubscribersAsync();
                if (subscribers == null || !subscribers.Any())
                {
                    return Ok(new { message = "No active subscribers found." });
                }

              
                var allRecentArticles = (await _articleService.GetMostPopularAsync(4)).ToList();

                var latestArticles = await _articleService.GetLatestAsync(10);
                allRecentArticles.AddRange(latestArticles);

                int sentCount = 0;

                foreach (var subscriber in subscribers)
                {
                    if (subscriber.User == null || string.IsNullOrEmpty(subscriber.User.Email))
                        continue;

                    // 2. Filter content tailored purely to subscriber preferences
                    var preferredArticles = allRecentArticles.Where(article =>
                    {
                        var favCategories = subscriber.SelectedCategoryIds?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                        var favAuthors = subscriber.SelectedAuthIds?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

                        bool matchesCategory = favCategories.Contains(article.CategoryId.ToString());
                        bool matchesAuthor = favAuthors.Contains(article.AuthorId.ToString());

                        return matchesCategory || matchesAuthor;
                    }).ToList();

                    // Fallback: Default to top recent articles if they haven't explicitly set filters yet
                    if (!preferredArticles.Any())
                    {
                        preferredArticles = allRecentArticles.Take(5).ToList();
                    }

                    if (!preferredArticles.Any())
                        continue;

                    // 3. Construct personalized newsletter markup
                    string customBodyHtml = _templateBuilder.GenerateNewsletterHtml(preferredArticles, "Ditt Veckovisa Nyhetsbrev");

                    // 4. Send dispatch via Smtp/Mailtrap
                    await _emailSender.SendEmailAsync(
                        subscriber.User.Email,
                        "CCN Nyhetsbrev - Veckans Uppdatering",
                        customBodyHtml
                    );

                    sentCount++;
                }

                return Ok(new { message = $"Personalized newsletter successfully sent to {sentCount} subscribers." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}