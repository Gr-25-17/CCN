using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;
using System.Security.Claims;

namespace NewsSite.Controllers;

/// <summary>
/// API endpoints for newsletter management (unsubscribe, preferences).
/// Used by email subscribers and the NewsletterSender Azure Function App.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NewsletterApiController : ControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterApiController> _logger;

    public NewsletterApiController(INewsletterService newsletterService, ILogger<NewsletterApiController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    /// <summary>
    /// Unsubscribe from newsletter using an unsubscribe token.
    /// Token is typically passed via email link: /api/newsletter/unsubscribe?token=...
    /// </summary>
    [HttpGet("unsubscribe")]
    public async Task<IActionResult> UnsubscribeByToken([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "Unsubscribe token is required" });
            }

            // Decode token to get userId
            var userId = DecodeUnsubscribeToken(token);
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(new { message = "Invalid or expired unsubscribe token" });
            }

            // Get current preferences
            var prefs = await _newsletterService.GetPreferencesAsync(userId);
            if (prefs == null)
            {
                return NotFound(new { message = "User preferences not found" });
            }

            // Disable newsletter
            prefs.ReceiveNewsletter = false;
            await _newsletterService.SavePreferencesAsync(userId, prefs);

            _logger.LogInformation($"User {userId} unsubscribed via token");

            return Ok(new { message = "You have been unsubscribed from our newsletter" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing unsubscribe: {ex.Message}");
            return StatusCode(500, new { message = "Error processing unsubscribe request" });
        }
    }

    /// <summary>
    /// Unsubscribe from newsletter (requires authentication).
    /// User must be logged in.
    /// </summary>
    [HttpPost("unsubscribe")]
    [Authorize]
    public async Task<IActionResult> UnsubscribeAuthenticated()
    {
        try
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var prefs = await _newsletterService.GetPreferencesAsync(userId);
            if (prefs == null)
            {
                return NotFound(new { message = "User preferences not found" });
            }

            prefs.ReceiveNewsletter = false;
            await _newsletterService.SavePreferencesAsync(userId, prefs);

            _logger.LogInformation($"User {userId} unsubscribed (authenticated)");

            return Ok(new { message = "You have been unsubscribed from our newsletter" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error unsubscribing authenticated user: {ex.Message}");
            return StatusCode(500, new { message = "Error processing your request" });
        }
    }

    /// <summary>
    /// Get newsletter preferences for the current authenticated user.
    /// </summary>
    [HttpGet("preferences")]
    [Authorize]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var prefs = await _newsletterService.GetPreferencesAsync(userId);
            return Ok(prefs);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching newsletter preferences: {ex.Message}");
            return StatusCode(500, new { message = "Error fetching preferences" });
        }
    }

    /// <summary>
    /// Update newsletter preferences for the current authenticated user.
    /// </summary>
    [HttpPost("preferences")]
    [Authorize]
    public async Task<IActionResult> UpdatePreferences([FromBody] NewsletterPreferencesUpdateDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var prefs = await _newsletterService.GetPreferencesAsync(userId);
            if (prefs == null)
            {
                return NotFound(new { message = "User preferences not found" });
            }

            // Update preferences
            prefs.ReceiveNewsletter = dto.ReceiveNewsletter;
            if (!string.IsNullOrEmpty(dto.Frequency))
            {
                prefs.Frequency = dto.Frequency; // Weekly, Monthly, etc.
            }
            if (dto.SelectedCategoryIds != null && dto.SelectedCategoryIds.Any())
            {
                prefs.SelectedCategoryIds = string.Join(",", dto.SelectedCategoryIds);
            }

            await _newsletterService.SavePreferencesAsync(userId, prefs);

            _logger.LogInformation($"User {userId} updated newsletter preferences");

            return Ok(new { message = "Preferences updated successfully", preferences = prefs });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating newsletter preferences: {ex.Message}");
            return StatusCode(500, new { message = "Error updating preferences" });
        }
    }

    /// <summary>
    /// Decodes an unsubscribe token to extract the userId.
    /// Token format: Base64(userId:expirationDate)
    /// </summary>
    private string? DecodeUnsubscribeToken(string token)
    {
        try
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':');
            if (parts.Length < 2)
            {
                return null;
            }

            var userId = parts[0];
            var expirationStr = parts[1];

            // Check if token has expired (30 days)
            if (!DateTime.TryParse(expirationStr, out var expiration) || DateTime.UtcNow > expiration)
            {
                _logger.LogWarning($"Unsubscribe token expired: {token}");
                return null;
            }

            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error decoding unsubscribe token: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// DTO for updating newsletter preferences.
/// </summary>
public class NewsletterPreferencesUpdateDto
{
    public bool ReceiveNewsletter { get; set; } = true;
    public string? Frequency { get; set; } // Weekly, Monthly, etc.
    public List<int>? SelectedCategoryIds { get; set; }
}
