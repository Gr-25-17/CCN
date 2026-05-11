using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Moq;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;

namespace Tests.Services;

public class BlobServiceTests
{
    // 1. Deklarera variablerna som klassfält
    private readonly Mock<BlobServiceClient> _mockBlobServiceClient;
    private readonly IBlobService _blobService;

    // 2. Skapa en konstruktor för att instansiera dem innan testerna körs
    public BlobServiceTests()
    {
        _mockBlobServiceClient = new Mock<BlobServiceClient>();

        // Skicka in det mockade objektet i din faktiska BlobService
        _blobService = new BlobService(_mockBlobServiceClient.Object);
    }

    [Fact]
    public async Task UploadToContainerAsync_ShouldReturnBlobUri()
    {
        // Arrange
        using var stream = new MemoryStream("fejkad-bild-data"u8.ToArray());
        var fileName = "testbild.webp";
        var contentType = "image/webp";
        var containerName = "articles-raw";

        var mockContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();
        var fakeUri = new Uri("https://dittstorage.blob.core.windows.net/articles-raw/testbild.webp");

        mockBlobClient.Setup(x => x.Uri).Returns(fakeUri);
        mockContainerClient.Setup(x => x.GetBlobClient(fileName)).Returns(mockBlobClient.Object);

        // Nu existerar _mockBlobServiceClient och fungerar korrekt
        _mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(containerName)).Returns(mockContainerClient.Object);

        // Act
        // Nu existerar _blobService och fungerar korrekt
        var result = await _blobService.UploadToContainerAsync(stream, fileName, contentType, containerName);

        // Assert
        Assert.Equal(fakeUri.ToString(), result);

        mockBlobClient.Verify(x => x.UploadAsync(
            It.IsAny<Stream>(),
            It.Is<BlobUploadOptions>(o => o.HttpHeaders.ContentType == contentType),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}