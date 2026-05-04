using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class BlobService(IConfiguration configuration, BlobServiceClient _blobServiceClient) : IBlobService
    {
        public async Task<string> UploadFileToContainer(FileUpLoadModel model)
        {
            using (var stream = model.File.OpenReadStream())
            {
                return await UploadStreamToContainer(stream, model.File.FileName);
            }
        }
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var connectionString = configuration["AzureWebJobsStorage"];
            if (string.IsNullOrEmpty(connectionString) || file == null || file.Length == 0)
            {
                return string.Empty;
            }

            var containerName = configuration["BlobContainerName"];
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            var blobClient = containerClient.GetBlobClient(Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);

            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadStreamToContainer(Stream stream, string fileName)
        {
            string connectionString = configuration["AzureWebJobsStorage"]!;
            string containerName = configuration["BlobContainerName"]!;

            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            await blobClient.UploadAsync(stream, true);

            return blobClient.Uri.ToString();
        }

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