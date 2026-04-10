using Microsoft.AspNetCore.Mvc;
using Moq;
using NewsSite.Controllers;
using NewsSite.Models.Entities;
using NewsSite.Services.Interfaces;
using FluentAssertions;
using Xunit;

namespace Tests.Controllers;

public class CategoryControllerTests
{
    private readonly Mock<IArticleService> _articleServiceMock;
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly CategoryController _controller;

    public CategoryControllerTests()
    {
        _articleServiceMock = new Mock<IArticleService>();
        _categoryServiceMock = new Mock<ICategoryService>();
        _controller = new CategoryController(_articleServiceMock.Object, _categoryServiceMock.Object);
    }

    [Fact]
    public async Task Index_ShouldReturnNotFound_WhenCategoryMissing()
    {
        _categoryServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Category>());
        var result = await _controller.Index("finns-inte");
        result.Should().BeOfType<NotFoundResult>();
    }
}
