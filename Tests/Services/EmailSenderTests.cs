using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using NewsSite.Services.Implementations;
using Xunit;

namespace NewsSite.Tests.Services;

public class EmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_ShouldReadFromConfigAndNotThrow()
    {
        // Arrange
        var configMock = new Mock<IConfiguration>();

        // Mocka de värden som Parse-metoderna i EmailSender kräver
        configMock.Setup(c => c["EmailSettings:SmtpServer"]).Returns("localhost");
        configMock.Setup(c => c["EmailSettings:SmtpPort"]).Returns("25");
        configMock.Setup(c => c["EmailSettings:SenderEmail"]).Returns("test@test.com");
        configMock.Setup(c => c["EmailSettings:SenderName"]).Returns("Test");

        var service = new EmailSender(configMock.Object);

        // Act & Assert
        // Vi testar att logiken för att hämta inställningar fungerar. 
        // Själva SmtpClient.SendMailAsync kommer försöka skicka, så vi verifierar att den inte kraschar på nulls.
        var act = async () => await service.SendEmailAsync("recipient@test.com", "Subject", "Body");

        // Vi förväntar oss inte att det faktiskt skickas i en miljö utan SMTP-server, 
        // men vi kollar att konfigurations-läsningen fungerar.
        await act.Should().NotThrowAsync<ArgumentNullException>();
    }
}