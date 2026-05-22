using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces;

public interface INewsletterPreferenceRepository
{
    Task<NewsletterPreference?> GetByUserIdAsync(string userId);
    Task<NewsletterPreference?>GetByTokenAsync(string token);
    Task SaveAsync(NewsletterPreference preference);

    Task<List<NewsletterPreference>> GetWeeklySubscribersAsync();
}