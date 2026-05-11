// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels;
using System.ComponentModel.DataAnnotations;


namespace NewsSite.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly INewsletterService _newsletterService;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            INewsletterService newsletterService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _newsletterService = newsletterService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// 

        [BindProperty]
        public NewsletterPreferencesViewModel ContentPreferences { get; set; } = new();
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 

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
        }



        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);

            var prefs = await _newsletterService.GetPreferencesAsync(user.Id);

            var categories = await _newsletterService.GetAllCategoriesAsync();
            var authors = await _newsletterService.GetAllAuthorsAsync();

            ContentPreferences = new NewsletterPreferencesViewModel
            {
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
            // 🔍 DEBUG: Skriv ut ALL data som kommer från formuläret
            System.Diagnostics.Debug.WriteLine($"=== FORMULÄR DATA ===");
            foreach (var key in Request.Form.Keys)
            {
                System.Diagnostics.Debug.WriteLine($"Key: {key} = {Request.Form[key]}");
            }

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

            // 🔍 DEBUG: Kolla Temp lists innan sparning
            System.Diagnostics.Debug.WriteLine($"=== TEMP LISTS ===");
            System.Diagnostics.Debug.WriteLine($"SelectedCategoryIdsTemp count: {ContentPreferences?.SelectedCategoryIdsTemp?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"SelectedAuthorIdsTemp count: {ContentPreferences?.SelectedAuthorIdsTemp?.Count ?? 0}");

            if (ContentPreferences?.SelectedCategoryIdsTemp != null && ContentPreferences.SelectedCategoryIdsTemp.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Category Temp values: {string.Join(",", ContentPreferences.SelectedCategoryIdsTemp)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Category Temp är NULL eller tom!");
            }

            if (ContentPreferences?.SelectedAuthorIdsTemp != null && ContentPreferences.SelectedAuthorIdsTemp.Any())
            {
                System.Diagnostics.Debug.WriteLine($"Author Temp values: {string.Join(",", ContentPreferences.SelectedAuthorIdsTemp)}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Author Temp är NULL eller tom!");
            }

            var existingPrefs = await _newsletterService.GetPreferencesAsync(user.Id);

            // 🔍 DEBUG: Kolla befintliga värden
            System.Diagnostics.Debug.WriteLine($"=== BEFINTLIGA VÄRDEN ===");
            System.Diagnostics.Debug.WriteLine($"Innan sparning - SelectedCategoryIds: {existingPrefs.SelectedCategoryIds}");
            System.Diagnostics.Debug.WriteLine($"Innan sparning - SelectedAuthorIds: {existingPrefs.SelectedAuthorIds}");

            // Konvertera listor till strings
            var newCategoryIds = string.Join(",", ContentPreferences?.SelectedCategoryIdsTemp ?? new List<int>());
            var newAuthorIds = string.Join(",", ContentPreferences?.SelectedAuthorIdsTemp ?? new List<string>());

            System.Diagnostics.Debug.WriteLine($"=== NYA VÄRDEN ===");
            System.Diagnostics.Debug.WriteLine($"Nya SelectedCategoryIds: {newCategoryIds}");
            System.Diagnostics.Debug.WriteLine($"Nya SelectedAuthorIds: {newAuthorIds}");

            existingPrefs.SelectedCategoryIds = newCategoryIds;
            existingPrefs.SelectedAuthorIds = newAuthorIds;

            await _newsletterService.SavePreferencesAsync(user.Id, existingPrefs);

            // 🔍 DEBUG: Bekräfta sparning
            System.Diagnostics.Debug.WriteLine($"=== SPARAT! ===");
            System.Diagnostics.Debug.WriteLine($"SelectedCategoryIds sparades som: {newCategoryIds}");
            System.Diagnostics.Debug.WriteLine($"SelectedAuthorIds sparades som: {newAuthorIds}");

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
