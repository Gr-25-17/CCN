using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NewsSite.Services.Interfaces;

namespace NewsSite.Functions;

public class ArchiveArticlesFunction(
    IArticleArchiveService articleArchiveService,
    ILogger<ArchiveArticlesFunction> logger)
{
    [Function(nameof(ArchiveArticlesFunction))]
    public async Task Run([TimerTrigger("0 0 0 * * *")] TimerInfo timer)
    {
        var archivedCount = await articleArchiveService.ArchiveArticlesOlderThanAsync();
        logger.LogInformation("Archived {Count} articles.", archivedCount);
    }
}
