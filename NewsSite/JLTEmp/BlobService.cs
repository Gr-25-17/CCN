using Azure.Storage.Blobs;


namespace NewsSite.JLTEmp
{
    public class BlobService : IBlobService
    {
        public readonly IConfiguration _configuration;

        public BlobService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> UploadFileToContainer(FileUpLoadModel model)
        {
            string connectionString = _configuration["AzureWebJobsStorage"]!;
            string containerName = _configuration["BlobContainerName"]!;
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            BlobContainerClient containerClient =
                                                blobServiceClient.GetBlobContainerClient(containerName);
            // Create the container if it does not exist
            await containerClient.CreateIfNotExistsAsync();
            BlobClient blobClient = containerClient.GetBlobClient(model.File.FileName);
            using (var stream = model.File.OpenReadStream())
            {
                try
                {
                    await blobClient.UploadAsync(stream, true);
                }
                catch (Exception ex)
                {

                    var err = ex.Message;
                    return err;
                }
            }
            return blobClient.Uri.ToString();
        }

    }
}
