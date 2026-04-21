using NewsSite.Mapping;
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

    public async Task<List<CategoryViewModel>> GetAllCategoriesAsync()
        => (await _categoryService.GetAllAsync()).ToList();

    public async Task<NewsletterPreferencesViewModel> GetPreferencesAsync(string userId)
        => (await _preferenceRepo.GetByUserIdAsync(userId)).ToNewsletterPreferencesViewModel(await GetAllCategoriesAsync());
    

    public async Task SavePreferencesAsync(string userId, NewsletterPreferencesViewModel preferences)
        => await _preferenceRepo.SaveAsync(preferences.ToNewsletterPreferenceEntity(userId));
}
