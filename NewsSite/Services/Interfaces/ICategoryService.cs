using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryViewModel>> GetAllAsync();
}
