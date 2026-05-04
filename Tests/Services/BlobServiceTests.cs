using Microsoft.Extensions.Configuration;
using Moq;
using NewsSite.Services.Implementations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Azure.Storage.Blobs;

namespace Tests.Services;

public class BlobServiceTests
{
    [Fact]
    public async Task UploadImageAsync_ShouldReturnEmpty_WhenConfigIsMissing()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["AzureWebJobsStorage"]).Returns(string.Empty);

        var blobServiceClientMock = new Mock<BlobServiceClient>();
        var service = new BlobService(configMock.Object, blobServiceClientMock.Object);
        var fileMock = new Mock<IFormFile>();

        var result = await service.UploadImageAsync(fileMock.Object);

        result.Should().BeEmpty();
    }

    //[Fact]
    //public async Task UploadFileToContainer_ShouldCallUploadStreamToContainer()
    //{
    //    var configMock = new Mock<IConfiguration>();
    //    var fileMock = new Mock<Microsoft.AspNetCore.Http.IFormFile>();
    //    var stream = new MemoryStream();
    //    fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
    //    fileMock.Setup(f => f.FileName).Returns("test.txt");
    //    var model = new NewsSite.Models.Entities.FileUpLoadModel { File = fileMock.Object };
    //    var service = new BlobService(configMock.Object);
    //    var result = await service.UploadFileToContainer(model);
    //    result.Should().NotBeNull();
    //}

    //[Fact]
    //public async Task UploadStreamToContainer_ShouldReturnUri()
    //{
    //    var configMock = new Mock<IConfiguration>();
    //    configMock.Setup(c => c["AzureWebJobsStorage"]).Returns("UseDevelopmentStorage=true");
    //    configMock.Setup(c => c["BlobContainerName"]).Returns("testcontainer");
    //    var service = new BlobService(configMock.Object);
    //    var stream = new MemoryStream(new byte[] { 1, 2, 3 });
    //    var result = await service.UploadStreamToContainer(stream, "file.txt");
    //    result.Should().NotBeNull();
    //}
}