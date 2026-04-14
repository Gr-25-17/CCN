using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces;

public interface INewsletterService
{
    Task<NewsletterPreferencesViewModel> GetPreferencesAsync(string userId);
    Task SavePreferencesAsync(string userId, NewsletterPreferencesViewModel preferences);
    Task<List<Category>> GetAllCategoriesAsync();
}