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
        public async Task<IActionResult> Index(string? search, string? roleFilter, bool? isDeletedFilter)
        {
            var model = await userService.GetUsersForAdminAsync();

            var query = model.Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalizedSearch = search.Trim();
                query = query.Where(user =>
                    user.FullName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                    user.Email.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(roleFilter))
            {
                query = query.Where(user => string.Equals(user.CurrentRole, roleFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (isDeletedFilter is not null)
            {
                query = query.Where(user => user.IsDeleted == isDeletedFilter.Value);
            }

            model.Users = query.ToList();
            ViewBag.AvailableRoles = model.AvailableRoles;
            model.Search = search;
            model.RoleFilter = roleFilter;
            model.IsDeletedFilter = isDeletedFilter;

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
        [HttpPost, ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RunImageMigration()
        {
            // Hämtar endast artiklar som behöver migreras (körs som SQL Query tack vare IQueryable)
            var articles = await _articleRepository.GetQueryable()
                .Where(a => !string.IsNullOrEmpty(a.ImageUrl) && !a.ImageUrl.EndsWith(".webp") && !a.ImageUrl.EndsWith(".svg"))
                .ToListAsync();

            var migratedCount = 0;
            var failedCount = 0;

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
                        migratedCount++;
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    logger.LogWarning(ex, "Kunde inte migrera bild för artikel {ArticleId}", article.Id);
                }
            }

            return Ok($"Migration slutförd. Migrerade: {migratedCount}. Misslyckade: {failedCount}.");
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
