using NewsSite.Services.Interfaces;
using SixLabors.ImageSharp;

namespace NewsSite.Services.Implementations
{
    public class ImageOrchestrationService(IBlobService blobService, IHttpClientFactory httpClientFactory) : IImageOrchestrationService
    {
        public async Task<(string FileName, string TempUrl, bool IsProcessing)> HandleIncomingImageAsync(Stream stream, string originalName, string contentType)
        {
            var format = await Image.IdentifyAsync(stream);
            if (format is null && contentType != "image/svg+xml")
                return (string.Empty, string.Empty, false);

            stream.Position = 0;

            var isSvg = contentType == "image/svg+xml" || originalName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase);
            var extension = Path.GetExtension(originalName).ToLower();
            var uniqueId = Guid.NewGuid().ToString("N");
            var blobName = $"{uniqueId}{extension}";

            var targetContainer = isSvg ? "articles-full" : "articles-raw";
            var tempUrl = await blobService.UploadToContainerAsync(stream, blobName, contentType, targetContainer);

            var finalFileName = isSvg ? blobName : $"{uniqueId}.webp";
            return (finalFileName, tempUrl, !isSvg);
        }

        public async Task<Stream?> FetchExternalImageAsync(string url)
        {
            var client = httpClientFactory.CreateClient();
            var response = await client.GetAsync(url);
            return response.IsSuccessStatusCode ? await response.Content.ReadAsStreamAsync() : null;
        }
    }
}
