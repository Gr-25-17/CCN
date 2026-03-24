using NewsSite.Models.Entities;

namespace NewsSite.Services
{
    public interface IBlobService
    {

       Task<string> UploadFileToContainer(FileUpLoadModel model);
    }
}
