using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<Category>> GetAllAsync();
}