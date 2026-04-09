using Microsoft.Extensions.Configuration;
using Moq;
using NewsSite.Services.Implementations;
using FluentAssertions;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace NewsSite.Tests.Services;

public class BlobServiceTests
{
    [Fact]
    public async Task UploadImageAsync_ShouldReturnEmpty_WhenConfigIsMissing()
    {
        var configMock = new Mock<IConfiguration>();
        configMock.Setup(c => c["AzureWebJobsStorage"]).Returns(string.Empty);

        var service = new BlobService(configMock.Object);
        var fileMock = new Mock<IFormFile>();

        var result = await service.UploadImageAsync(fileMock.Object);

        result.Should().BeEmpty();
    }
}