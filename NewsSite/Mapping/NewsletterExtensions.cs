using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class NewsletterExtensions
    {
        private const string DefaultFrequency = "Weekly";

        public static NewsletterPreferencesViewModel ToNewsletterPreferencesViewModel(this NewsletterPreference? prefs, List<CategoryViewModel> availableCategories)
            => prefs is null
                ? new NewsletterPreferencesViewModel
                {
                    ReceiveNewsletter = false,
                    Frequency = DefaultFrequency,
                    SelectedCategoryIds = null,
                    AvailableCategories = availableCategories,
                    UnsubscribeToken = null,
                    IsUnsubscribed = false,
                    UnsubscribedAt = null
                }
                : new NewsletterPreferencesViewModel
                {
                    ReceiveNewsletter = prefs.ReceiveNewsletter,
                    Frequency = prefs.Frequency,
                    SelectedCategoryIds = prefs.SelectedCategoryIds,
                    AvailableCategories = availableCategories,
                    UnsubscribeToken = prefs.UnsubscribeToken,
                    IsUnsubscribed = prefs.IsUnsubscribed,
                    UnsubscribedAt = prefs.UnsubscribedAt
                };

        public static NewsletterPreference ToNewsletterPreferenceEntity(this NewsletterPreferencesViewModel prefs, string userId, string? existingToken = null)
            => new()
            {
                UserId = userId,
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                SelectedAuthIds = prefs.SelectedAuthorIds,
                UpdatedAt = DateTime.UtcNow,
                UnsubscribeToken = string.IsNullOrWhiteSpace(existingToken) ? GenerateToken() : existingToken
            };

        private static string GenerateToken()
            => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
    }
}
