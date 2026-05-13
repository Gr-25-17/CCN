using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace NewsSite.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountApiController> _logger;

        public AccountApiController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountApiController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null && user.IsDeleted)
            {
                return BadRequest(new { error = "Detta konto är inaktiverat eller raderat." });
            }

            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in via AJAX.");
                return Ok(new { success = true, message = "Du har loggats in framgångsrikt!" });
            }

            if (result.RequiresTwoFactor)
            {
                return BadRequest(new { error = "Two-factor authentication is required." });
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return BadRequest(new { error = "Ditt konto är låst. Försök igen senare." });
            }

            return BadRequest(new { error = "Ogiltigt e-postadress eller lösenord." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .FirstOrDefault() ?? "Ogiltiga uppgifter.";

                return BadRequest(new { error });
            }

            if (await _userManager.FindByEmailAsync(request.Email) is not null)
            {
                return BadRequest(new { error = "En användare med denna e-postadress finns redan." });
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var error = createResult.Errors.FirstOrDefault()?.Description ?? "Registreringen misslyckades.";
                return BadRequest(new { error });
            }

            await _userManager.AddToRoleAsync(user, "Reader");
            await _signInManager.SignInAsync(user, isPersistent: false);

            _logger.LogInformation("User registered via AJAX.");

            return Ok(new { success = true, message = "Kontot har skapats och du är nu inloggad." });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out via AJAX.");
            return Ok(new { success = true, message = "Du har loggats ut." });
        }

        public class LoginRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public class RegisterRequest
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            [MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [Required]
            [Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
