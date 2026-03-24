namespace NewsSite.JLTEmp
{
    public interface IBlobService
    {

       Task<string> UploadFileToContainer(FileUpLoadModel model);
    }
}
