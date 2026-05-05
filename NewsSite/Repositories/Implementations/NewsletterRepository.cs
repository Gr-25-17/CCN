using Microsoft.EntityFrameworkCore;
using NewsSite.Data;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;

namespace NewsSite.Repositories.Implementations;

public class NewsletterRepository : INewsletterRepository
{
    private readonly ApplicationDbContext _context;

    public NewsletterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Newsletter>> GetAllAsync()
    {
        return await _context.Newsletters
            .Include(n => n.CreatedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Newsletter>> GetActiveAsync()
    {
        return await _context.Newsletters
            .Where(n => !n.IsDeleted)
            .Include(n => n.CreatedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Newsletter>> GetByStatusAsync(string status)
    {
        return await _context.Newsletters
            .Where(n => !n.IsDeleted && n.Status == status)
            .Include(n => n.CreatedBy)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Newsletter?> GetByIdAsync(int id)
    {
        return await _context.Newsletters
            .Include(n => n.CreatedBy)
            .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
    }

    public async Task<Newsletter> CreateAsync(Newsletter newsletter)
    {
        newsletter.CreatedAt = DateTime.UtcNow;
        newsletter.UpdatedAt = DateTime.UtcNow;

        _context.Newsletters.Add(newsletter);
        await _context.SaveChangesAsync();

        return newsletter;
    }

    public async Task<Newsletter> UpdateAsync(Newsletter newsletter)
    {
        newsletter.UpdatedAt = DateTime.UtcNow;

        _context.Newsletters.Update(newsletter);
        await _context.SaveChangesAsync();

        return newsletter;
    }

    public async Task DeleteAsync(int id)
    {
        var newsletter = await _context.Newsletters.FindAsync(id);
        if (newsletter != null)
        {
            _context.Newsletters.Remove(newsletter);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int id)
    {
        var newsletter = await _context.Newsletters.FindAsync(id);
        if (newsletter != null)
        {
            newsletter.IsDeleted = true;
            newsletter.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Newsletter>> GetPendingSendAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.Newsletters
            .Where(n => !n.IsDeleted && n.Status == "Scheduled" && n.ScheduledSendTime <= now)
            .Include(n => n.CreatedBy)
            .OrderBy(n => n.ScheduledSendTime)
            .ToListAsync();
    }

    public async Task<List<Newsletter>> GetScheduledBetweenAsync(DateTime startTime, DateTime endTime)
    {
        return await _context.Newsletters
            .Where(n => !n.IsDeleted && n.Status == "Scheduled" && 
                        n.ScheduledSendTime >= startTime && n.ScheduledSendTime <= endTime)
            .Include(n => n.CreatedBy)
            .OrderBy(n => n.ScheduledSendTime)
            .ToListAsync();
    }
}
