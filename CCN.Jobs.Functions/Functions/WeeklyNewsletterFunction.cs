using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace CCN.Jobs.Functions.Functions;

public class WeeklyNewsletterFunction(
    IHttpClientFactory httpClientFactory,
    ILogger<WeeklyNewsletterFunction> logger,
    IConfiguration configuration)
{
    [Function("WeeklyNewsletter")]
    public async Task Run(
        [TimerTrigger("0 0 9 * * 1", RunOnStartup = false)] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Weekly newsletter trigger started at {Time}", DateTime.UtcNow);

        var appUrl = configuration["NewsletterSettings:AppUrl"];
        var apiKey = configuration["NewsletterSettings:ApiKey"];

        if (string.IsNullOrWhiteSpace(appUrl) || string.IsNullOrWhiteSpace(apiKey))
        {
            logger.LogError("Missing NewsletterSettings:AppUrl or NewsletterSettings:ApiKey configuration.");
            return;
        }

        var requestUrl = $"{appUrl.TrimEnd('/')}/api/newsletter/send-weekly";
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
        request.Headers.Add("X-API-Key", apiKey);

        using var client = httpClientFactory.CreateClient();
        var response = await client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Newsletter API failed with status code {StatusCode}", response.StatusCode);
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Weekly newsletter completed successfully: {Response}", responseBody);
    }
}
