using Moq;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;
using FluentAssertions;
using Xunit;

namespace NewsSite.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _service = new UserService(_repoMock.Object);
    }

    [Fact]
    public async Task SoftDeleteUserAsync_ShouldSetIsDeletedToTrue()
    {
        var user = new ApplicationUser { Id = "user1", IsDeleted = false };
        _repoMock.Setup(r => r.GetUserByIdAsync("user1")).ReturnsAsync(user);
        _repoMock.Setup(r => r.UpdateUserDetailsAsync(user)).ReturnsAsync(true);

        var result = await _service.SoftDeleteUserAsync("user1");

        result.Should().BeTrue();
        user.IsDeleted.Should().BeTrue();
    }
}