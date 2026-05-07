using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using NewsSite.Models.Entities;
using NewsSite.Repositories.Interfaces;
using NewsSite.Services.Implementations;

namespace Tests.Services
{

	public class UserServiceTests
	{
		private readonly Mock<IUserRepository> _repoMock;
		private readonly UserService _service;

		public UserServiceTests()
		{
			_repoMock = new Mock<IUserRepository>();
			_service = new UserService(_repoMock.Object);
		}
        /*[Fact]
        public async Task UpdateUserRoleAsync_ShouldReturnTrue_WhenRepoSucceeds()
        {
            _repoMock.Setup(r => r.UpdateUserRoleAsync("u1", "Admin")).ReturnsAsync(true);

            var result = await _service.UpdateUserRoleAsync("u1", "Admin");

            result.Should().BeTrue();
            _repoMock.Verify(r => r.UpdateUserRoleAsync("u1", "Admin"), Times.Once);
        }

        [Fact]
        public async Task GetUsersForAdminAsync_ShouldMapUsersCorrectly()
        {
            var users = new List<ApplicationUser>
    {
        new() { Id = "u1", FirstName = "Kalle", LastName = "Anka", Email = "kalle@test.com", IsDeleted = false }
    };
            var roles = new List<IdentityRole> { new("Admin") };

            _repoMock.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(users);
            _repoMock.Setup(r => r.GetAllRolesAsync()).ReturnsAsync(roles);
            _repoMock.Setup(r => r.GetUserRolesAsync(users[0])).ReturnsAsync(new List<string> { "Admin" });

            var result = await _service.GetUsersForAdminAsync();

            result.Users.Should().HaveCount(1);
            result.Users[0].FullName.Should().Be("Kalle Anka");
            result.Users[0].CurrentRole.Should().Be("Admin");
            result.AvailableRoles.Should().Contain(r => r.Text == "Admin");
        }
        [Fact]
		public async Task AnonymizeUserAsync_ShouldScrubPersonalDataButKeepRecord()
		{
			var user = new ApplicationUser
			{
				Id = "user-123",
				Email = "testuser@example.com",
				FirstName = "Kalle",
				LastName = "Anka"
			};
			_repoMock.Setup(r => r.GetUserByIdAsync("user-123")).ReturnsAsync(user);
			_repoMock.Setup(r => r.UpdateUserDetailsAsync(user)).ReturnsAsync(true);

			var result = await _service.AnonymizeUserAsync("user-123");
			result.Should().BeTrue();
			user.FirstName.Should().Be("Anonymous");
			user.LastName.Should().Be("User");
			user.Email.Should().Be($"deleted_{user.Id.Substring(0, 8)}@anonymized.com");
			user.IsDeleted.Should().BeTrue();


		}
		[Fact]
		public async Task RestoreUserAsync_ShouldSetIsDeletedToFalse()
		{
			var user = new ApplicationUser { Id = "s1", IsDeleted = true };
			_repoMock.Setup(r => r.GetUserByIdAsync("s1")).ReturnsAsync(user);
			_repoMock.Setup(r => r.UpdateUserDetailsAsync(user)).ReturnsAsync(true);

			var result = await _service.RestoreUserAsync("s1");

			result.Should().BeTrue();
			user.IsDeleted.Should().BeFalse();
        }

		[Fact]
		public async Task AnonymizeUserAsync_ShouldReturnFalse_WhenUserNotFound()
		{
			_repoMock.Setup(r => r.GetUserByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null!);
			var result = await _service.AnonymizeUserAsync("unknown");
			result.Should().BeFalse();
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
		}*/
	}
}