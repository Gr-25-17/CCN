using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();
    }
}