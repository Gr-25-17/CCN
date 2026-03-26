using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;

namespace NewsSite.ViewComponents;

public class NavbarCategoryViewComponent : ViewComponent
{
    private readonly ICategoryService _context;

    public NavbarCategoryViewComponent(ICategoryService context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.GetAllAsync();

        return View(categories);
    }
}