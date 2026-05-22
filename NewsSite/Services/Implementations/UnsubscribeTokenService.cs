using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using System.Security.Cryptography;

namespace NewsSite.Services.Implementations;

public class UnsubscribeTokenService : IUnsubscribeTokenService
{
    private readonly INewsletterPreferenceRepository _preferenceRepo;
    private readonly ILogger<UnsubscribeTokenService> _logger;

    public UnsubscribeTokenService(
        INewsletterPreferenceRepository preferenceRepo,
        ILogger<UnsubscribeTokenService> logger)
    {
        _preferenceRepo = preferenceRepo;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(string userId)
    {
        var preference = await _preferenceRepo.GetByUserIdAsync(userId);

        if (preference == null)
        {
            preference = new NewsletterPreference { UserId = userId };
        }

        var token = GenerateSecureToken();
        preference.UnsubscribeToken = token;
        preference.UpdatedAt = DateTime.UtcNow;

        await _preferenceRepo.SaveAsync(preference);

        return token;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var preference = await _preferenceRepo.GetByTokenAsync(token);
        return preference != null;
    }

    public async Task<NewsletterPreference?> GetPreferenceByTokenAsync(string token)
    {
        return await _preferenceRepo.GetByTokenAsync(token);
    }

    public async Task UnsubscribeAsync(string token, string? reason = null)
    {
        var preference = await _preferenceRepo.GetByTokenAsync(token);
        if (preference == null)
        {
            throw new ArgumentException("Invalid token");
        }

        preference.ReceiveNewsletter = false;
        preference.IsUnsubscribed = true;
        preference.UnsubscribedAt = DateTime.UtcNow;
        preference.UnsubscribeReason = reason;
        preference.UpdatedAt = DateTime.UtcNow;

        await _preferenceRepo.SaveAsync(preference);
    }

    public async Task ReactivateAsync(string userId)
    {
        var preference = await _preferenceRepo.GetByUserIdAsync(userId);

        if (preference == null) return;

        preference.ReceiveNewsletter = true;
        preference.IsUnsubscribed = false;
        preference.UnsubscribedAt = null;
        preference.UnsubscribeReason = null;
        preference.UpdatedAt = DateTime.UtcNow;

        preference.UnsubscribeToken = GenerateSecureToken();

        await _preferenceRepo.SaveAsync(preference);
    }

    public async Task SuspendAsync(string token, int days)
    {
        var preference = await _preferenceRepo.GetByTokenAsync(token);
        if (preference == null)
        {
            throw new ArgumentException("Invalid token");
        }

        preference.ReceiveNewsletter = false;
        preference.UpdatedAt = DateTime.UtcNow;

        await _preferenceRepo.SaveAsync(preference);
    }

    private string GenerateSecureToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}