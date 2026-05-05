using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Services.Interfaces;

public interface INewsletterManagementService
{
    /// <summary>
    /// Get all newsletters for admin management
    /// </summary>
    Task<List<NewsletterItemViewModel>> GetAllNewslettersAsync();

    /// <summary>
    /// Get a specific newsletter for editing
    /// </summary>
    Task<NewsletterManagementViewModel?> GetNewsletterForEditAsync(int id);

    /// <summary>
    /// Create a new newsletter
    /// </summary>
    Task<NewsletterManagementViewModel> CreateNewsletterAsync(NewsletterManagementViewModel viewModel, string userId);

    /// <summary>
    /// Update an existing newsletter
    /// </summary>
    Task<NewsletterManagementViewModel> UpdateNewsletterAsync(NewsletterManagementViewModel viewModel);

    /// <summary>
    /// Delete a newsletter
    /// </summary>
    Task DeleteNewsletterAsync(int id);

    /// <summary>
    /// Get a newsletter ready for preview (with sample articles)
    /// </summary>
    Task<NewsletterPreviewViewModel> GetNewsletterPreviewAsync(int id);

    /// <summary>
    /// Manually trigger a newsletter to be sent immediately
    /// </summary>
    Task<bool> SendNewsletterNowAsync(int id);

    /// <summary>
    /// Get newsletter statistics/metadata
    /// </summary>
    Task<NewsletterStatsViewModel> GetStatsAsync();
}
