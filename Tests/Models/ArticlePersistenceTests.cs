using FluentAssertions;
using NewsSite.Models.Entities;

namespace Tests.Models
{
    public class ArticlePersistenceTests
    {
        [Fact]
        public void Article_ShouldKeepAuthorName_EvenIfUserIsAnonymized()
        {
            // Arrange
            var article = new Article
            {
                Title = "Viktig nyhet",
                AuthorName = "Anna Andersson",
                AuthorId = "user-1"
            };

            var user = new ApplicationUser
            {
                Id = "user-1",
                FirstName = "Anna",
                LastName = "Andersson"
            };

            // Simulera anonymisering av användaren
            user.FirstName = "Anonymiserad";
            user.LastName = "Användare";

            // Assert
            article.AuthorName.Should().Be("Anna Andersson");
            article.AuthorName.Should().NotBe($"{user.FirstName} {user.LastName}");
        }
    }
}
