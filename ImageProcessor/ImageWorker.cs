using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using System.Net;

namespace ImageProcessor;

public class ImageWorker(BlobServiceClient blobService, ILogger<ImageWorker> logger)
{
    // Arkitekturell notis: Konstanter är ok för strikta interna domänregler, 
    // men överväg att lägga "articles-raw" i local.settings.json på sikt.
    private const string RawContainer = "articles-raw";
    private readonly (string Container, int Width)[] _targets = [
        ("articles-full", 0),
        ("articles-med", 800),
        ("articles-min", 200)
    ];

    [Function(nameof(ProcessArticleImage))]
    public async Task ProcessArticleImage(
        [BlobTrigger($"{RawContainer}/{{name}}", Connection = "AzureWebJobsStorage")] Stream stream,
        string name)
    {
        logger.LogInformation("BlobTrigger startad för bild: {ImageName}", name);
        await ProcessImageInternalAsync(stream, name);
    }

    [Function(nameof(RepairExistingImages))]
    public async Task<HttpResponseData> RepairExistingImages(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
    {
        logger.LogInformation("Startar reparation av existerande bilder...");
        var containerClient = blobService.GetBlobContainerClient(RawContainer);
        int count = 0;

        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);

            // Kritiskt fix: using-deklaration för att förhindra minnesläckage
            // Hämta responsen (ej IDisposable)
            var downloadResult = await blobClient.DownloadStreamingAsync();

            // Plocka ut strömmen och applicera 'using' på den för säker hantering
            using var contentStream = downloadResult.Value.Content;

            await ProcessImageInternalAsync(contentStream, blobItem.Name);
            count++;

            
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Reparationen klar! Bearbetade {count} bilder.");
        return response;
    }

    private async Task ProcessImageInternalAsync(Stream stream, string name)
    {
        try
        {
            using var image = await Image.LoadAsync(stream);
            var baseName = Path.GetFileNameWithoutExtension(name);
            var encoder = new WebpEncoder { Quality = 100 };

            foreach (var (container, width) in _targets)
            {
                // Säkerställ att målcontainern existerar (krävs vid ny infrastruktur)
                var targetContainerClient = blobService.GetBlobContainerClient(container);
                await targetContainerClient.CreateIfNotExistsAsync();

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
                var client = targetContainerClient.GetBlobClient($"{baseName}.webp");

                await client.UploadAsync(outputStream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = "image/webp" }
                });
            }

            // Kritiskt fix: DeleteIfExistsAsync hanterar race conditions mycket bättre än DeleteAsync
            await blobService.GetBlobContainerClient(RawContainer).GetBlobClient(name).DeleteIfExistsAsync();
            logger.LogInformation("Lyckades konvertera och spara: {ImageName}", name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fel vid bearbetning av {ImageName}", name);
        }
    }
}