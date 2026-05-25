using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/newsletter")]
    public class NewsLetterController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IWeeklyNewsletterService _weeklyNewsletterService;
        private readonly ILogger<NewsLetterController> _logger;

        public NewsLetterController(
            IConfiguration configuration,
            IWeeklyNewsletterService weeklyNewsletterService,
            ILogger<NewsLetterController> logger)
        {
            _configuration = configuration;
            _weeklyNewsletterService = weeklyNewsletterService;
            _logger = logger;
        }

        [HttpPost("send-weekly")]
        public async Task<IActionResult> SendWeeklyNewsletter()
        {
            var apiKey = Request.Headers["X-API-Key"].ToString();
            var expectedKey = _configuration["NewsletterSettings:ApiKey"];

            if (string.IsNullOrEmpty(apiKey) || apiKey != expectedKey)
            {
                return Unauthorized(new { error = "Invalid API key" });
            }

            try
            {
                var sentCount = await _weeklyNewsletterService.SendWeeklyNewsletterAsync();

                return Ok(new
                {
                    message = sentCount > 0
                        ? $"Weekly newsletter sent to {sentCount} recipients."
                        : "No eligible recipients found."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Weekly newsletter dispatch failed.");
                return StatusCode(500, new { error = "Weekly newsletter dispatch failed.", detail = ex.Message });
            }
        }
    }
}
