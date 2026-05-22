using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class DisabledBlobService(ILogger<DisabledBlobService> logger) : IBlobService
{
    public Task<string> UploadToContainerAsync(Stream stream, string fileName, string contentType, string containerName)
    {
        logger.LogError("Blob upload requested but storage is not configured. Set AzureWebJobsStorage or ConnectionStrings:AzureWebJobsStorage.");
        throw new InvalidOperationException("Blob storage is not configured in the current environment.");
    }
}
