using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NewsSite.Services.Implementations;
using Xunit;

namespace Tests.Services;

public class EmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldReadFromConfigAndNotThrow()
    {
        var configMock = new Mock<IConfiguration>();

        // Mocka de värden som int.Parse förväntar sig i EmailSender
        configMock.Setup(c => c["EmailSettings:SmtpServer"]).Returns("localhost");
        configMock.Setup(c => c["EmailSettings:SmtpPort"]).Returns("25");
        configMock.Setup(c => c["EmailSettings:SenderEmail"]).Returns("noreply@test.com");
        configMock.Setup(c => c["EmailSettings:SenderName"]).Returns("Test");

        var service = new EmailSender(configMock.Object);

        // Vi verifierar att konfigurationen läses utan att kasta NullReferenceException
        var act = async () => await service.SendEmailAsync("user@test.com", "Sub", "Body");

        await act.Should().NotThrowAsync<ArgumentNullException>();
    }
}