using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(
        IUserService userService,
        IArticleRepository _articleRepository,
        IImageOrchestrationService _imageOrchestrationService,
        ILocalToSqlServerMigrationService localToSqlServerMigrationService,
        ILogger<AdminController> logger) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await userService.GetUsersForAdminAsync();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(string userId, string newRole)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(newRole))
            {
                return RedirectToAction(nameof(Index));
            }

            var success = await userService.UpdateUserRoleAsync(userId, newRole);
            if (!success)
            {
                TempData["Error"] = "Gick inte att uppdatera rollen.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction(nameof(Index));
            }

            var success = await userService.SoftDeleteUserAsync(userId);

            if (!success)
            {
                TempData["Error"] = "Kunde inte radera användaren.";
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return RedirectToAction(nameof(Index));

            var success = await userService.RestoreUserAsync(userId);
            if (!success) TempData["Error"] = "Kunde inte återställa användaren.";

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunImageMigration()
        {
            // Hämtar endast artiklar som behöver migreras (körs som SQL Query tack vare IQueryable)
            var articles = await _articleRepository.GetQueryable()
                .Where(a => !string.IsNullOrEmpty(a.ImageUrl) && !a.ImageUrl.EndsWith(".webp") && !a.ImageUrl.EndsWith(".svg"))
                .ToListAsync();

            foreach (var article in articles)
            {
                try
                {
                    using var stream = await _imageOrchestrationService.FetchExternalImageAsync(article.ImageUrl!);
                    if (stream == null) continue;

                    // Fix CS8130: Ta emot hela tupeln i en variabel istället för dekonstruktion
                    var result = await _imageOrchestrationService.HandleIncomingImageAsync(
                        stream, article.ImageUrl, "application/octet-stream");

                    if (!string.IsNullOrEmpty(result.FileName))
                    {
                        article.ImageUrl = result.FileName;
                        await _articleRepository.UpdateAsync(article);
                    }
                }
                catch (Exception ex)
                {
                    // Implementera ILogger här framöver för att fånga upp specifika artiklar som misslyckas 
                    // _logger.LogWarning(ex, "Kunde inte migrera bild för artikel {Id}", article.Id);
                }
            }

            return Ok("Migration slutförd. Azure Functions bearbetar bilderna asynkront.");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RunLocalToLexiconMigration(CancellationToken cancellationToken)
        {
            try
            {
                var migratedRows = await localToSqlServerMigrationService.MigrateAsync(cancellationToken);
                TempData["Success"] = $"Datamigrering klar. {migratedRows} rader synkades till extern SQL Server.";
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Datamigrering kunde inte starta på grund av konfiguration eller anslutning.");
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
