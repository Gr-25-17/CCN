using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace ImageProcessor
{
    public class ImageWorker(BlobServiceClient blobService, ILogger<ImageWorker> logger)
    {
        private const string RawContainer = "articles-raw";
        private readonly (string Container, int Width)[] _targets = [
            ("articles-full", 0),
            ("articles-med", 800),
            ("articles-min", 200)
        ];

        // Denna reagerar på NYA filer
        [Function("ProcessArticleImage")]
        public async Task Run([BlobTrigger($"{RawContainer}/{{name}}", Connection = "AzureWebJobsStorage")] Stream stream, string name)
        {
            await ProcessImageInternal(stream, name);
        }

        // NY FUNKTION: Denna anropar du manuellt för att fixa EXISTERANDE filer
        [Function("RepairExistingImages")]
        public async Task<HttpResponseData> Repair([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("Startar reparation av existerande bilder...");
            var containerClient = blobService.GetBlobContainerClient(RawContainer);
            int count = 0;

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                var downloadResult = await blobClient.DownloadStreamingAsync();

                await ProcessImageInternal(downloadResult.Value.Content, blobItem.Name);
                count++;
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Reparationen klar! Bearbetade {count} bilder.");
            return response;
        }

        // Gemensam logik för konvertering
        private async Task ProcessImageInternal(Stream stream, string name)
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
                        HttpHeaders = new BlobHttpHeaders { ContentType = "image/webp" }
                    });
                }
                // Ta bort originalet när vi är klara[cite: 1]
                await blobService.GetBlobContainerClient(RawContainer).GetBlobClient(name).DeleteAsync();
                logger.LogInformation("Lyckades konvertera: {ImageName}", name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fel vid bearbetning av {ImageName}", name);
            }
        }
    }
}