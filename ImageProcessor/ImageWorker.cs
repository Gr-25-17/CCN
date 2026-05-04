using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using Microsoft.Azure.Functions.Worker;
using Azure.Storage.Blobs.Models;

namespace ImageProcessor
{
    public class ImageWorker(BlobServiceClient blobService, ILogger<ImageWorker> logger)
    {
        private const string RawContainer = "articles-raw";
        private readonly(string Container, int Width)[] _targets=[
            ("articles-full",0),
            ("articles-med",800),
            ("articles-min",200)
        ];
        [Function("ProcessArticleImage")]
        public async Task Run([BlobTrigger($"{RawContainer}/{{name}}")]Stream stream, string name)
        {
            try
            {
                using var image = await Image.LoadAsync(stream);
                var baseName = Path.GetFileNameWithoutExtension(name);
                var encoder = new WebpEncoder() { Quality = 100 };

                foreach (var (container, width) in _targets)
                {
                    using var outputStream = new MemoryStream();
                    if (width > 0)
                    {
                        using var clone = image.Clone(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(width, 0),
                            Mode = ResizeMode.Max
                        }));
                        await clone.SaveAsWebpAsync(outputStream, encoder);
                    }
                    else
                    {
                        await image.SaveAsWebpAsync(outputStream, encoder);
                    }
                    outputStream.Position = 0;
                    var client = blobService.GetBlobContainerClient(container).GetBlobClient($"{baseName}.webp");
                    await client.UploadAsync(outputStream, new BlobUploadOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = "image/webp"
                        }
                    });
                }
                await blobService.GetBlobContainerClient(RawContainer).GetBlobClient(name).DeleteAsync();
                logger.LogInformation("Processed and converted {ImageName} to WebP.", name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process {ImageName}. The file might be corrupt.", name);
            }
        }
    }
}
