using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces
{
    public interface IBlobService
    {

       Task<string> UploadFileToContainer(FileUpLoadModel model);
    }
}
