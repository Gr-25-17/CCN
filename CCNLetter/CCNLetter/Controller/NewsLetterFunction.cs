using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CCNLetter.Controller
{
    internal class NewsLetterFunction
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NewsLetterFunction> _logger;
        private readonly IConfiguration _configuration;

        // Inject HttpClient and Configuration instead of the old direct email services
        public NewsLetterFunction(
            HttpClient httpClient,
            ILogger<NewsLetterFunction> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuration = configuration;
        }

        // Set RunOnStartup = false so it doesn't instantly fire when debugging launches
        [Function("WeeklyNewsletter")]
        public async Task Run(
            [TimerTrigger("0 0 9 * * 1", RunOnStartup = false)] TimerInfo timer,
            CancellationToken ct)
        {
            _logger.LogInformation("Weekly newsletter trigger started");

            // Pull these values from your Function App's local.settings.json
            var appUrl = _configuration["NewsletterSettings:AppUrl"]; 
            var apiKey = _configuration["NewsletterSettings:ApiKey"];   

            if (string.IsNullOrWhiteSpace(appUrl) || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogError("Missing AppUrl or ApiKey configuration in local.settings.json");
                return;
            }

            try
            {
                // Match the exact route we added to your MVC Controller
                var requestUrl = $"{appUrl.TrimEnd('/')}/api/newsletter/send-weekly";

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);

                // Add the header so the MVC App authenticates the call successfully
                request.Headers.Add("X-API-Key", apiKey);

                _logger.LogInformation("Sending trigger request to MVC app at: {Url}", requestUrl);
                var response = await _httpClient.SendAsync(request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Newsletter API failed with status code: {StatusCode}", response.StatusCode);
                    return;
                }

                var responseBody = await response.Content.ReadAsStringAsync(ct);
                _logger.LogInformation("Weekly newsletter completed successfully: {Response}", responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to communicate with MVC Newsletter API.");
            }
        }
    }
}