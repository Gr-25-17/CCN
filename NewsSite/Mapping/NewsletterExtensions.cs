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
                    AvailableCategories = availableCategories
                };
            }

            return new NewsletterPreferencesViewModel
            {
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                AvailableCategories = availableCategories
            };
        }
        public static NewsletterPreference ToNewsletterPreferenceEntity(this NewsletterPreferencesViewModel prefs, string userId)
        {
            return new NewsletterPreference
            {
                UserId = userId,
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                SelectedAuthIds = prefs.SelectedAuthorIds,
                UpdatedAt = DateTime.UtcNow,
                UnsubscribeToken = Guid.NewGuid().ToString()
            };
        }
    }
}