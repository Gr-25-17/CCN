using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;

namespace NewsSite.ViewComponents;

public class NavbarCategoryViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NavbarCategoryViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Id)
            .ToListAsync();

        return View(categories);
    }
}