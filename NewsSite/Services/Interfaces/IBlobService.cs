using NewsSite.Models.Entities;
using System.IO;

namespace NewsSite.Services.Interfaces
{
    public interface IBlobService
    {
        Task<string> UploadFileToContainer(FileUpLoadModel model);
        Task<string> UploadStreamToContainer(Stream stream, string fileName);
    }
}