using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces;

public interface INewsletterPreferenceRepository
{
    Task<NewsletterPreference?> GetByUserIdAsync(string userId);
    Task SaveAsync(NewsletterPreference preference);
}