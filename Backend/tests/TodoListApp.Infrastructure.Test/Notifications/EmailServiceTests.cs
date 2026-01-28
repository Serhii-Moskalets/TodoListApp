using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Notifications;
using TodoListApp.Infrastructure.Notifications.Services;

namespace TodoListApp.Infrastructure.Test.Notifications;

/// <summary>
/// Provides unit tests for <see cref="EmailService"/> to verify its coordination logic.
/// </summary>
public class EmailServiceTests
{
    private readonly Mock<IEmailSender> _senderMock;
    private readonly Mock<IEmailTemplateProvider> _templateProviderMock;
    private readonly EmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailServiceTests"/> class and sets up mocks.
    /// </summary>
    public EmailServiceTests()
    {
        this._senderMock = new Mock<IEmailSender>();
        this._templateProviderMock = new Mock<IEmailTemplateProvider>();

        this._emailService = new EmailService(this._senderMock.Object, this._templateProviderMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendConfirmationEmailAsync"/> correctly orchestrates
    /// the template generation and email delivery processes.
    /// </summary>
    /// <remarks>
    /// The test ensures that:
    /// <list type="number">
    /// <item>The <see cref="IEmailTemplateProvider"/> is called with the expected template name.</item>
    /// <item>The <see cref="IEmailSender"/> receives the specific HTML body returned by the provider.</item>
    /// </list>
    /// </remarks>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendConfirmationEmailAsync_ShouldCallProviderAndSender()
    {
        // Arrange
        const string email = "test@user.com";
        this._templateProviderMock.Setup(x => x.GetEmailTemplateAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync("<html>Generated Body</html>");

        // Act
        await this._emailService.SendConfirmationEmailAsync(email, "John", "https://link.com", CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(x => x.GetEmailTemplateAsync("email_verification", It.IsAny<Dictionary<string, string>>()), Times.Once);

        this._senderMock.Verify(x => x.SendAsync(email, It.IsAny<string>(), "<html>Generated Body</html>", It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="EmailService.SendEmailChangeConfirmationAsync"/> correctly
    /// coordinates template processing and delivery for email change requests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendEmailChangeConfirmationAsync_ShouldCallProviderWithCorrectTemplate()
    {
        // Arrange
        const string email = "new-email@user.com";
        const string userName = "john";
        const string changeLink = "https://link.com/change";
        const string expectedBody = "<html>Email Change Body</html>";

        this._templateProviderMock
            .Setup(x => x.GetEmailTemplateAsync(
                "email_change_confirmation",
                It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(expectedBody);

        // Act
        await this._emailService.SendEmailChangeConfirmationAsync(email, userName, changeLink, CancellationToken.None);

        // Assert
        this._templateProviderMock.Verify(
            x => x.GetEmailTemplateAsync(
                "email_change_confirmation",
                It.Is<Dictionary<string, string>>(
                    dict =>
                    dict["USER_NAME"] == userName &&
                    dict["CHANGE_LINK"] == changeLink)),
            Times.Once);

        this._senderMock.Verify(
            x => x.SendAsync(
                email,
                "Confirm your new email address",
                expectedBody,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
