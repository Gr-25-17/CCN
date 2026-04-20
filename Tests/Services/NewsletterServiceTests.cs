using Moq;
using NewsSite.Models.ViewModels;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using NewsSite.Services.Interfaces;
using NewsSite.Models.Entities;

namespace Tests.Services
{
    public class NewsletterServiceTests
    {
        [Fact]
        public async Task GetAllCategoriesAsync_ReturnsCategories()
        {
            var mockCategoryService = new Mock<ICategoryService>();
            mockCategoryService.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<CategoryViewModel> { new CategoryViewModel() });
            var service = new NewsletterService(Mock.Of<INewsletterPreferenceRepository>(), mockCategoryService.Object);

            var result = await service.GetAllCategoriesAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPreferencesAsync_ReturnsPreferences()
        {
            var mockRepo = new Mock<INewsletterPreferenceRepository>();
            mockRepo.Setup(x => x.GetByUserIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new NewsletterPreference());
            var mockCategoryService = new Mock<ICategoryService>();
            mockCategoryService.Setup(x => x.GetAllAsync())
                .ReturnsAsync(new List<CategoryViewModel>());
            var service = new NewsletterService(mockRepo.Object, mockCategoryService.Object);

            var result = await service.GetPreferencesAsync("user1");

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SavePreferencesAsync_CallsRepoSave()
        {
            var mockRepo = new Mock<INewsletterPreferenceRepository>();
            mockRepo.Setup(x => x.SaveAsync(It.IsAny<NewsletterPreference>()))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var service = new NewsletterService(mockRepo.Object, Mock.Of<ICategoryService>());

            await service.SavePreferencesAsync("user1", new NewsletterPreferencesViewModel());

            mockRepo.Verify(x => x.SaveAsync(It.IsAny<NewsletterPreference>()), Times.Once);
        }
    }
}
