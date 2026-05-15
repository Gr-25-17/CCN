using NewsSite.Models.Entities;

namespace NewsSite.Services.Interfaces;

public interface IUnsubscribeTokenService
{
    
    Task<string> GenerateTokenAsync(string userId);

   
    Task<bool> ValidateTokenAsync(string token);

   
    Task<NewsletterPreference?> GetPreferenceByTokenAsync(string token);

    
    Task UnsubscribeAsync(string token, string? reason = null);

    
    Task ReactivateAsync(string userId);

    Task SuspendAsync(string token, int days);
}