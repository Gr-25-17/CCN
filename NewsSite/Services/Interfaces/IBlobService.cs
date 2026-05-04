using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileToContainer(FileUpLoadModel model);
        Task<string> UploadStreamToContainer(Stream stream, string fileName);
        Task<string> UploadToContainerAsync(Stream stream, string blobName, string contentType, string containerName);
    }
}