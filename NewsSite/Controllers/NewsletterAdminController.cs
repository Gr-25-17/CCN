using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.Entities;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers;

/// <summary>
/// Admin controller for managing newsletters.
/// Allows admins to create, edit, schedule, and send newsletters.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("Admin/Newsletters")]
public class NewsletterAdminController : Controller
{
    private readonly INewsletterManagementService _newsletterService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NewsletterAdminController(
        INewsletterManagementService newsletterService,
        UserManager<ApplicationUser> userManager)
    {
        _newsletterService = newsletterService;
        _userManager = userManager;
    }

    /// <summary>
    /// Display list of all newsletters with stats
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var newsletters = await _newsletterService.GetAllNewslettersAsync();
        var stats = await _newsletterService.GetStatsAsync();

        var viewModel = new NewsletterListViewModel
        {
            Newsletters = newsletters
        };

        ViewBag.Stats = stats;
        return View(viewModel);
    }

    /// <summary>
    /// Display form to create a new newsletter
    /// </summary>
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var viewModel = new NewsletterManagementViewModel();
        return View("Edit", viewModel);
    }

    /// <summary>
    /// Display form to edit an existing newsletter
    /// </summary>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var viewModel = await _newsletterService.GetNewsletterForEditAsync(id);
        if (viewModel == null)
        {
            TempData["Error"] = "Newsletter not found.";
            return RedirectToAction(nameof(Index));
        }

        return View(viewModel);
    }

    /// <summary>
    /// Save newsletter (create or update)
    /// </summary>
    [HttpPost("Save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(NewsletterManagementViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View("Edit", viewModel);
        }

        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            if (viewModel.Id == 0)
            {
                // Create new newsletter
                await _newsletterService.CreateNewsletterAsync(viewModel, user.Id);
                TempData["Success"] = "Newsletter created successfully.";
            }
            else
            {
                // Update existing newsletter
                await _newsletterService.UpdateNewsletterAsync(viewModel);
                TempData["Success"] = "Newsletter updated successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error saving newsletter: {ex.Message}";
            return View("Edit", viewModel);
        }
    }

    /// <summary>
    /// Preview a newsletter before sending
    /// </summary>
    [HttpGet("Preview/{id}")]
    public async Task<IActionResult> Preview(int id)
    {
        try
        {
            var preview = await _newsletterService.GetNewsletterPreviewAsync(id);
            return View(preview);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error loading preview: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Schedule a newsletter for sending at a specific time
    /// </summary>
    [HttpPost("Schedule/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Schedule(int id, DateTime scheduledTime)
    {
        try
        {
            var newsletter = await _newsletterService.GetNewsletterForEditAsync(id);
            if (newsletter == null)
            {
                TempData["Error"] = "Newsletter not found.";
                return RedirectToAction(nameof(Index));
            }

            if (scheduledTime <= DateTime.UtcNow)
            {
                TempData["Error"] = "Scheduled time must be in the future.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            newsletter.Status = "Scheduled";
            newsletter.ScheduledSendTime = scheduledTime;
            await _newsletterService.UpdateNewsletterAsync(newsletter);

            TempData["Success"] = $"Newsletter scheduled for {scheduledTime:g}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error scheduling newsletter: {ex.Message}";
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    /// <summary>
    /// Send a newsletter immediately
    /// </summary>
    [HttpPost("SendNow/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendNow(int id)
    {
        try
        {
            var success = await _newsletterService.SendNewsletterNowAsync(id);
            if (success)
            {
                TempData["Success"] = "Newsletter has been queued for immediate sending.";
            }
            else
            {
                TempData["Error"] = "Newsletter not found.";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error sending newsletter: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// Delete a newsletter
    /// </summary>
    [HttpPost("Delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _newsletterService.DeleteNewsletterAsync(id);
            TempData["Success"] = "Newsletter deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting newsletter: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}
