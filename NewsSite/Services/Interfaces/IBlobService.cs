using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadToContainerAsync(Stream stream, string fileName, string contentType, string containerName);
    }
}