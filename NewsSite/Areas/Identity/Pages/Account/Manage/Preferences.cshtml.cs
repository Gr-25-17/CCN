using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Areas.Identity.Pages.Account.Manage;

public class PreferencesModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly INewsletterService _newsletterService;

    public PreferencesModel(
        UserManager<ApplicationUser> userManager,
        INewsletterService newsletterService)
    {
        _userManager = userManager;
        _newsletterService = newsletterService;
    }

    [BindProperty]
    public NewsletterPreferencesViewModel Input { get; set; } = new();

    [TempData]
    public string StatusMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        Input = await _newsletterService.GetPreferencesAsync(user.Id);
        Input.AvailableCategories = await _newsletterService.GetAllCategoriesAsync();
        Input.AvailableAuthors = await _newsletterService.GetAllAuthorsAsync();

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
            Input.AvailableCategories = await _newsletterService.GetAllCategoriesAsync();
            Input.AvailableAuthors = await _newsletterService.GetAllAuthorsAsync();
            return Page();
        }

        await _newsletterService.SavePreferencesAsync(user.Id, Input);

        StatusMessage = "Dina nyhetsbrevsinställningar har sparats.";

        return RedirectToPage();
    }
}