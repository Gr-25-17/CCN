using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;

namespace NewsSite.Mapping
{
    public static class NewsletterExtensions
    {
        public static NewsletterPreferencesViewModel ToNewsletterPreferencesViewModel(this NewsletterPreference? prefs, List<CategoryViewModel> availableCategories)
        {
            if (prefs == null)
            {
                return new NewsletterPreferencesViewModel
                {
                    ReceiveNewsletter = false,
                    Frequency = "Weekly",
                    SelectedCategoryIds = null,
                    AvailableCategories = availableCategories,
                    UnsubscribeToken = null,     
                    IsUnsubscribed = false,       
                    UnsubscribedAt = null         
                };
            }

            return new NewsletterPreferencesViewModel
            {
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                AvailableCategories = availableCategories,
                UnsubscribeToken = prefs.UnsubscribeToken,     
                IsUnsubscribed = prefs.IsUnsubscribed,         
                UnsubscribedAt = prefs.UnsubscribedAt          
            };
        }
        public static NewsletterPreference ToNewsletterPreferenceEntity(this NewsletterPreferencesViewModel prefs, string userId, string? existingToken = null)
        {
            return new NewsletterPreference
            {
                UserId = userId,
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                SelectedAuthIds = prefs.SelectedAuthorIds,
                UpdatedAt = DateTime.UtcNow,

                UnsubscribeToken = !string.IsNullOrEmpty(existingToken)
                    ? existingToken
                    : GenerateToken()
            };
        }

        private static string GenerateToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }
    }
}