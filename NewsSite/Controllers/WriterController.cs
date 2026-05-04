using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsSite.Services.Interfaces;
using NewsSite.Models.ViewModels;
using NewsSite.Models.Entities;
using System.Security.Claims;

namespace NewsSite.Controllers
{
    [Authorize(Roles = "Admin,Editor,Writer")]
    public class WriterController(IArticleService articleService, IBlobService blobService, IHttpClientFactory httpClientFactory, IImageOrchestrationService imageOrchestrationService) : Controller
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

            await HandleImageUpload(model);

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

            await HandleImageUpload(model);

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
        public async Task<IActionResult> UploadImageAjax(IFormFile? file, [FromForm] string externalUrl)
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

        private async Task HandleImageUpload(ArticleViewModel model)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var fileUploadModel = new FileUpLoadModel { File = model.ImageFile };
                var blobUrl = await blobService.UploadFileToContainer(fileUploadModel);

                if (blobUrl.StartsWith("https"))
                {
                    model.ImageUrl = blobUrl;
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.ImageExternalUrl))
            {
                try
                {
                    var client = httpClientFactory.CreateClient();
                    var response = await client.GetAsync(model.ImageExternalUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var contentType = response.Content.Headers.ContentType?.MediaType;
                        if (contentType != null && contentType.StartsWith("image/"))
                        {
                            var uri = new Uri(model.ImageExternalUrl);
                            var fileName = Path.GetFileName(uri.LocalPath);

                            if (string.IsNullOrWhiteSpace(fileName))
                            {
                                fileName = Guid.NewGuid().ToString() + "." + contentType.Split('/')[1];
                            }

                            using (var stream = await response.Content.ReadAsStreamAsync())
                            {
                                var blobUrl = await blobService.UploadStreamToContainer(stream, fileName);

                                if (blobUrl.StartsWith("https"))
                                {
                                    model.ImageUrl = blobUrl;
                                }
                            }
                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
}