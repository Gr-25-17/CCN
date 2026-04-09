using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.ViewModels;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController(IUserService userService) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await userService.GetUsersForAdminAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
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
    }
}