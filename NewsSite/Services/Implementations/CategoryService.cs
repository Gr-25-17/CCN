using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations
{
    public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
    {
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await categoryRepository.GetAllAsync();
        }
    }
}