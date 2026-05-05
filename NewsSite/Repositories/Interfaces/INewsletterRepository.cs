using NewsSite.Models.Entities;

namespace NewsSite.Repositories.Interfaces;

public interface INewsletterRepository
{
    /// <summary>
    /// Get all newsletters (including deleted) for admin view
    /// </summary>
    Task<List<Newsletter>> GetAllAsync();

    /// <summary>
    /// Get all non-deleted newsletters
    /// </summary>
    Task<List<Newsletter>> GetActiveAsync();

    /// <summary>
    /// Get newsletters by status
    /// </summary>
    Task<List<Newsletter>> GetByStatusAsync(string status);

    /// <summary>
    /// Get a single newsletter by ID
    /// </summary>
    Task<Newsletter?> GetByIdAsync(int id);

    /// <summary>
    /// Create a new newsletter
    /// </summary>
    Task<Newsletter> CreateAsync(Newsletter newsletter);

    /// <summary>
    /// Update an existing newsletter
    /// </summary>
    Task<Newsletter> UpdateAsync(Newsletter newsletter);

    /// <summary>
    /// Delete a newsletter (hard delete from database)
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Soft delete a newsletter
    /// </summary>
    Task SoftDeleteAsync(int id);

    /// <summary>
    /// Get newsletters scheduled for sending (Scheduled status, time passed)
    /// </summary>
    Task<List<Newsletter>> GetPendingSendAsync();

    /// <summary>
    /// Get newsletters scheduled for a specific time range
    /// </summary>
    Task<List<Newsletter>> GetScheduledBetweenAsync(DateTime startTime, DateTime endTime);
}
