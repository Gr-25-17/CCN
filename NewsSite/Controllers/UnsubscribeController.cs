using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers;

[ApiController]
[Route("[controller]")]
public class UnsubscribeController : ControllerBase
{
    private readonly IUnsubscribeTokenService _tokenService;
    private readonly ILogger<UnsubscribeController> _logger;

    public UnsubscribeController(
        IUnsubscribeTokenService tokenService,
        ILogger<UnsubscribeController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeRequest request)
    {
        if (request == null || string.IsNullOrEmpty(request.Token))
        {
            _logger.LogWarning("Unsubscribe attempted with invalid token");
            return BadRequest(new { success = false, message = "Invalid token" });
        }

        try
        {
            switch (request.Action)
            {
                case "UnsubscribeAll":
                    await _tokenService.UnsubscribeAsync(request.Token, request.UnsubscribeReason);
                    return Ok(new { success = true, message = "Unsubscribed successfully" });

                case "Suspend30Days":
                    await _tokenService.SuspendAsync(request.Token, 30);
                    return Ok(new { success = true, message = "Subscription paused for 30 days" });

                default:
                    return BadRequest(new { success = false, message = "Invalid action" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during unsubscribe");
            return StatusCode(500, new { success = false, message = "Internal server error" });
        }
    }
}