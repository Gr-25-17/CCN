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
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null && user.IsDeleted)
                return BadRequest(new { error = "Detta konto är inaktiverat eller raderat." });

            var result = await _signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                request.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
                return Ok(new { success = true, message = "Du har loggats in framgångsrikt!" });

            if (result.RequiresTwoFactor)
                return BadRequest(new { error = "Two-factor authentication is required." });

            if (result.IsLockedOut)
                return BadRequest(new { error = "Ditt konto är låst. Försök igen senare." });

            if (user is not null && !await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest(new { error = "Du måste bekräfta din e-postadress innan du kan logga in." });

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
                return BadRequest(new { error = "En användare med denna e-postadress finns redan." });

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DateOfBirth = request.DateOfBirth
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);

            if (!createResult.Succeeded)
            {
                var error = createResult.Errors.FirstOrDefault()?.Description ?? "Registreringen misslyckades.";
                return BadRequest(new { error });
            }

            await _userManager.AddToRoleAsync(user, "Reader");

            _logger.LogInformation("User registered via AJAX.");

            return Ok(new
            {
                success = true,
                message = "Kontot har skapats. Bekräfta din e-postadress innan du loggar in."
            });
        }

        public class LoginRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public class RegisterRequest
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            public string LastName { get; set; } = string.Empty;

            [Required]
            public DateTime DateOfBirth { get; set; }

            [Required, MinLength(6)]
            public string Password { get; set; } = string.Empty;

            [Required, Compare(nameof(Password))]
            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}
