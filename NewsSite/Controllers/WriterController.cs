using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin,Editor,Writer")]
    public class WriterController(IArticleService articleService, IImageOrchestrationService imageOrchestrationService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canSeeAll = User.IsInRole("Admin") || User.IsInRole("Editor");

            var articles = await articleService.GetBackendArticlesAsync(userId!, canSeeAll);
            return View(articles);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = await articleService.GetEditorModelAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await articleService.GetEditorModelAsync();
                model.Categories = categories.Categories;
                return View(model);
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await articleService.CreateAsync(model, userId!);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canSeeAll = User.IsInRole("Admin") || User.IsInRole("Editor");

            var model = await articleService.GetForEditAsync(id, userId!, canSeeAll);
            if (model == null)
            {
                return Forbid();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ArticleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await articleService.GetEditorModelAsync();
                model.Categories = categories.Categories;
                return View(model);
            }


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canSeeAll = User.IsInRole("Admin") || User.IsInRole("Editor");

            var success = await articleService.UpdateAsync(model, userId!, canSeeAll);
            if (!success)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDraft(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canSeeAll = User.IsInRole("Admin") || User.IsInRole("Editor");
            var success = await articleService.DeleteDraftAsync(id, userId!, canSeeAll);

            if (!success)
            {
                return Forbid();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UploadImageAjax(IFormFile? file, [FromForm] string? externalUrl)
        {
            Stream? stream = null;
            string fileName = string.Empty;
            string contentType = string.Empty;

            try
            {
                if (file is { Length: > 0 })
                {
                    stream = file.OpenReadStream();
                    fileName = file.FileName;
                    contentType = file.ContentType;
                }
                else if (!string.IsNullOrWhiteSpace(externalUrl))
                {
                    stream = await imageOrchestrationService.FetchExternalImageAsync(externalUrl);
                    if (stream is null) return BadRequest("Kunde inte hämta resurser från URL:en");

                    fileName = Path.GetFileName(new Uri(externalUrl).LocalPath);
                    contentType = "application/octet-stream";
                }
                else
                {
                    return BadRequest("Ingen giltig fil eller URL tillhandahölls.");
                }
                var (finalName, tempUrl, processing) = await imageOrchestrationService.HandleIncomingImageAsync(stream, fileName, contentType);
                if (string.IsNullOrEmpty(finalName))
                    return BadRequest("Filen är antingen korrupt eller inte ett giltigt bildformat.");

                return Ok(new { fileName = finalName, temporaryUrl = tempUrl, isProcessing = processing });
            }
            finally
            {
                stream?.Dispose();

            }
        }

        
        
    }
}
