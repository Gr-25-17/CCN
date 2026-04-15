using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;

namespace NewsSite.Services.Implementations;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterPreferenceRepository _preferenceRepo;
    private readonly ICategoryService _categoryService;

    public NewsletterService(
        INewsletterPreferenceRepository preferenceRepo,
        ICategoryService categoryService)
    {
        _preferenceRepo = preferenceRepo;
        _categoryService = categoryService;
    }

    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        var categories = await _categoryService.GetAllAsync();
        return categories.ToList();
    }

    public async Task<NewsletterPreferencesViewModel> GetPreferencesAsync(string userId)
    {
        var prefs = await _preferenceRepo.GetByUserIdAsync(userId);

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
        var prefs = new NewsletterPreference
        {
            UserId = userId,
            ReceiveNewsletter = preferences.ReceiveNewsletter,
            Frequency = preferences.Frequency,
            SelectedCategoryIds = preferences.SelectedCategoryIds,
            UpdatedAt = DateTime.UtcNow,
            UnsubscribeToken = Guid.NewGuid().ToString()
        };

        await _preferenceRepo.SaveAsync(prefs);
    }
}