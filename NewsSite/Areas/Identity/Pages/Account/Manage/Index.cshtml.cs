#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Implementations;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INewsletterService _newsletterService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly ISubscriptionRepository _subscriptionRepository;
        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            INewsletterService newsletterService, ISubscriptionService subscriptionService, ISubscriptionRepository subscriptionRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _newsletterService = newsletterService;
            _subscriptionService = subscriptionService;
            _subscriptionRepository = subscriptionRepository;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public NewsletterPreferencesViewModel ContentPreferences { get; set; } = new();

        public SubscriptionInfoViewModel SubscriptionInfo { get; set; } = new();
        public class InputModel
        {
            [Required]
            public string FirstName { get; set; }

            [Required]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime DateOfBirth { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                DateOfBirth = user.DateOfBirth,
                PhoneNumber = phoneNumber
            };

            var prefs = await _newsletterService.GetPreferencesAsync(user.Id);
            ContentPreferences.ReceiveNewsletter = prefs.ReceiveNewsletter;
            ContentPreferences.Frequency = "weekly";
            ContentPreferences.UnsubscribeToken = prefs.UnsubscribeToken;
            ContentPreferences.IsUnsubscribed = prefs.IsUnsubscribed;

            var activeSubscription = await _subscriptionRepository.GetActiveSubscriptionAsync(user.Id);

            if (activeSubscription != null)
            {
                SubscriptionInfo = new SubscriptionInfoViewModel
                {
                    HasActiveSubscription = true,
                    StartDate = activeSubscription.StartDate,
                    EndDate = activeSubscription.EndDate,
                    PlanName = activeSubscription.Type?.Name ?? "Premium",
                    Price = activeSubscription.Type?.Price
                };
            }
            else
            {
                SubscriptionInfo = new SubscriptionInfoViewModel
                {
                    HasActiveSubscription = false
                };
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);  // ← Hämtar ALLT nu

            var prefs = await _newsletterService.GetPreferencesAsync(user.Id);
            var categories = await _newsletterService.GetAllCategoriesAsync();
            var authors = await _newsletterService.GetAllAuthorsAsync();

            ContentPreferences = new NewsletterPreferencesViewModel
            {
                ReceiveNewsletter = prefs.ReceiveNewsletter,
                Frequency = prefs.Frequency,
                UnsubscribeToken = prefs.UnsubscribeToken,
                IsUnsubscribed = prefs.IsUnsubscribed,
                SelectedCategoryIds = prefs.SelectedCategoryIds,
                SelectedAuthorIds = prefs.SelectedAuthorIds,
                AvailableCategories = categories,
                AvailableAuthors = authors,
                SelectedCategoryIdsTemp = prefs.SelectedCategoryIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList() ?? new List<int>(),
                SelectedAuthorIdsTemp = prefs.SelectedAuthorIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .ToList() ?? new List<string>()
            };

            return Page(); 
        }



        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.DateOfBirth = Input.DateOfBirth;
            await _userManager.UpdateAsync(user);

            var existingPrefs = await _newsletterService.GetPreferencesAsync(user.Id);

            var receiveNewsletter = Request.Form["ContentPreferences.ReceiveNewsletter"] == "true";

            existingPrefs.ReceiveNewsletter = receiveNewsletter;
            existingPrefs.Frequency = "Weekly";  
            existingPrefs.IsUnsubscribed = false;  

            
            var newCategoryIds = string.Join(",", ContentPreferences?.SelectedCategoryIdsTemp ?? new List<int>());
            var newAuthorIds = string.Join(",", ContentPreferences?.SelectedAuthorIdsTemp ?? new List<string>());

            existingPrefs.SelectedCategoryIds = newCategoryIds;
            existingPrefs.SelectedAuthorIds = newAuthorIds;

            await _newsletterService.SavePreferencesAsync(user.Id, existingPrefs);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}