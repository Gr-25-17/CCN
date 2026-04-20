using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;
using FluentAssertions;

namespace Tests.Controllers;

public class BlobDemoControllerTests
{
    [Fact]
    public async Task Upload_ShouldReturnContent_WhenFileNotSelected()
    {
        var blobServiceMock = new Mock<IBlobService>();
        var controller = new BlobDemoController(blobServiceMock.Object);
        var model = new FileUpLoadModel { File = null };
        var result = await controller.Upload(model);
        result.Should().BeOfType<ContentResult>();
    }

    //[Fact]
    //public async Task Upload_ShouldRedirectToShowImage_WhenSuccess()
    //{
    //    var blobServiceMock = new Mock<IBlobService>();
    //    blobServiceMock.Setup(s => s.UploadFileToContainer(It.IsAny<FileUpLoadModel>())).ReturnsAsync("https://test");
    //    var controller = new BlobDemoController(blobServiceMock.Object);
    //    controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(new DefaultHttpContext(), Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
    //    var model = new FileUpLoadModel { File = new Mock<IFormFile>().Object };
    //    var result = await controller.Upload(model);
    //    result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("ShowImage");
    //}

    //[Fact]
    //public async Task Upload_ShouldRedirectToError_WhenNotHttps()
    //{
    //    var blobServiceMock = new Mock<IBlobService>();
    //    blobServiceMock.Setup(s => s.UploadFileToContainer(It.IsAny<FileUpLoadModel>())).ReturnsAsync("not-a-url");
    //    var controller = new BlobDemoController(blobServiceMock.Object);
    //    var model = new FileUpLoadModel { File = new Mock<IFormFile>().Object };
    //    var result = await controller.Upload(model);
    //    result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Error");
    //}
}
