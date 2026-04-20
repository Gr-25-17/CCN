//using Azure;
//using Azure.Data.Tables;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using NewsSite.Services.Implementations;

//namespace Tests.Services
//{
//    public class GoldServiceTests
//    {
//        [Fact]
//        public async Task GetLatestPricesAsync_ReturnsList_WhenNoException()
//        {
//            var tableClientMock = new Mock<TableClient>(MockBehavior.Strict);
//            tableClientMock.Setup(x => x.QueryAsync<It.IsAnyType>(It.IsAny<string>(), null, null, default))
//                .Returns(new Mock<AsyncPageable<It.IsAnyType>>().Object);
//            var configMock = new Mock<IConfiguration>();
//            configMock.Setup(c => c["AzureWebJobsStorage"]).Returns("UseDevelopmentStorage=true");
//            var loggerMock = new Mock<ILogger<GoldService>>();

//            var service = new GoldService(configMock.Object, loggerMock.Object);
//            var result = await service.GetLatestPricesAsync();
//            Assert.NotNull(result);
//        }
//    }
//}
