using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class NewsletterService : INewsletterService
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryService _categoryService;

    public NewsletterService(ApplicationDbContext context, ICategoryService categoryService)
    {
        _context = context;
        _categoryService = categoryService;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        return categories.ToList();
    }

    public async Task<NewsletterPreferencesViewModel> GetPreferencesAsync(string userId)
    {
        var prefs = await _context.NewsletterPreferences
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (prefs == null)
        {
            return new NewsletterPreferencesViewModel
            {
                ReceiveNewsletter = false,
                Frequency = "Weekly",
                SelectedCategoryIds = null,
                AvailableCategories = await GetAllCategoriesAsync()
            };
        }

        return new NewsletterPreferencesViewModel
        {
            ReceiveNewsletter = prefs.ReceiveNewsletter,
            Frequency = prefs.Frequency,
            SelectedCategoryIds = prefs.SelectedCategoryIds,
            AvailableCategories = await GetAllCategoriesAsync()
        };
    }

    public async Task SavePreferencesAsync(string userId, NewsletterPreferencesViewModel preferences)
    {
        var existing = await _context.NewsletterPreferences
            .FirstOrDefaultAsync(n => n.UserId == userId);

        if (existing == null)
        {
            var newPrefs = new NewsletterPreference
            {
                UserId = userId,
                ReceiveNewsletter = preferences.ReceiveNewsletter,
                Frequency = preferences.Frequency,
                SelectedCategoryIds = preferences.SelectedCategoryIds,
                UpdatedAt = DateTime.UtcNow,
                UnsubscribeToken = Guid.NewGuid().ToString()
            };
            _context.NewsletterPreferences.Add(newPrefs);
        }
        else
        {
            existing.ReceiveNewsletter = preferences.ReceiveNewsletter;
            existing.Frequency = preferences.Frequency;
            existing.SelectedCategoryIds = preferences.SelectedCategoryIds;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.NewsletterPreferences.Update(existing);
        }

        await _context.SaveChangesAsync();
    }
}