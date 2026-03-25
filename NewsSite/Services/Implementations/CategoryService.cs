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
        // TODO: Ta bort detta när databasen fungerar
        return await Task.FromResult(new List<Category>
        {
            new Category { Id = 1, Name = "Sweden" },
            new Category { Id = 2, Name = "World" },
            new Category { Id = 3, Name = "Sport" },
            new Category { Id = 4, Name = "Economy" },
            new Category { Id = 5, Name = "Weather" }
        });

        /* Original kod
        return await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();
        */
    }
}