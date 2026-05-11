using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations;

public class NewsletterPreferenceRepository : INewsletterPreferenceRepository
{
    private readonly ApplicationDbContext _context;

    public NewsletterPreferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NewsletterPreference?> GetByUserIdAsync(string userId)
    {
        return await _context.NewsletterPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task SaveAsync(NewsletterPreference preference)
    {
        var existing = await GetByUserIdAsync(preference.UserId);

        if (existing == null)
        {
            _context.NewsletterPreferences.Add(preference);
        }
        else
        {
            existing.ReceiveNewsletter = preference.ReceiveNewsletter;
            existing.Frequency = preference.Frequency;
            existing.SelectedCategoryIds = preference.SelectedCategoryIds;
            existing.SelectedAuthIds = preference.SelectedAuthIds;
            existing.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(preference.UnsubscribeToken))
            {
                existing.UnsubscribeToken = preference.UnsubscribeToken;
            }
            _context.Entry(existing).Property(x => x.SelectedAuthIds).IsModified = true;
        }

        await _context.SaveChangesAsync();
    }
}