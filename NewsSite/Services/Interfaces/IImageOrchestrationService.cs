namespace NewsSite.Services.Interfaces
{
    public interface IImageOrchestrationService
    {
        Task<(string FileName,string TempUrl,bool IsProcessing)> HandleIncomingImageAsync(Stream stream, string originalName, string contentType);
        Task<Stream?> FetchExternalImageAsync(string url);
    }
}
