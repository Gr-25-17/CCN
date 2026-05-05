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
                UpdatedAt = DateTime.UtcNow,
                UnsubscribeToken = Guid.NewGuid().ToString()
            };
        }

        /// <summary>
        /// Parse comma-separated category IDs from the newsletter
        /// </summary>
        public static List<int> GetSelectedCategoryIds(this Newsletter newsletter)
        {
            if (string.IsNullOrEmpty(newsletter.SelectedCategoryIds))
                return new List<int>();

            return newsletter.SelectedCategoryIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id.Trim(), out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();
        }

        /// <summary>
        /// Check if newsletter has any specific categories selected
        /// </summary>
        public static bool HasCategorySelection(this Newsletter newsletter)
        {
            return !string.IsNullOrEmpty(newsletter.SelectedCategoryIds) && 
                   newsletter.SelectedCategoryIds.Split(',').Any(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}