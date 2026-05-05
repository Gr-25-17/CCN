using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class BlobService(BlobServiceClient _blobServiceClient) : IBlobService
    {
        async Task<string> IBlobService.UploadToContainerAsync(Stream stream, string fileName, string contentType, string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var options = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };
            await blobClient.UploadAsync(stream, options);

            return blobClient.Uri.ToString();
        }
    }
}