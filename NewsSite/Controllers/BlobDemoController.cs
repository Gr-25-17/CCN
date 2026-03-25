using Microsoft.AspNetCore.Mvc;
using NewsSite.Models.Entities;
using NewsSite.Services;


namespace NewsSite.Controllers
{
    public class BlobDemoController : Controller
    {

        private readonly IBlobService _blobService;

        public BlobDemoController(IBlobService blobService)
        {
            _blobService = blobService;
        }
        public IActionResult Index()
        {
            return View("Upload");
        }

        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(FileUpLoadModel model)
        {
            if (model.File == null || model.File.Length == 0)
            {
                return Content("File not selected");
            }
            var returnString = await _blobService.UploadFileToContainer(model);
            if (returnString.StartsWith("https"))
            {
                TempData["UrlString"] = returnString;
                return RedirectToAction("ShowImage");
            }

            return RedirectToAction("Error");
        }

        public IActionResult ShowImage()
        {
            var peek = TempData.Peek("UrlString");
            return View(peek);
        }
    }
}

