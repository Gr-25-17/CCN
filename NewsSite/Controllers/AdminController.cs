using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels;

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
                TempData["Error"] = "Kunde inte uppdatera rollen.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}