using TodoListApp.Application.Abstractions.Interfaces.Notifications;

namespace TodoListApp.Infrastructure.Notifications.Services;

/// <summary>
/// High-level service that coordinates email template processing and delivery.
/// </summary>
/// <param name="sender">The SMTP sender implementation.</param>
/// <param name="templateProvider">The template engine for loading HTML files.</param>
public class EmailService
    (IEmailSender sender,
    IEmailTemplateProvider templateProvider) : IEmailService
{
    /// <summary>
    /// Prepares and sends a registration confirmation email to a new user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The name of the user to be used in the greeting.</param>
    /// <param name="confirmLink">The URL for email verification.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendConfirmationEmailAsync(
        string toEmail,
        string userName,
        string confirmLink,
        CancellationToken ct = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "USER_NAME", userName },
            { "VERIFY_LINK", confirmLink },
        };

        var body = await templateProvider.GetEmailTemplateAsync("email_verification", placeholders);

        await sender.SendAsync(toEmail, "Registration confirmation", body, ct);
    }

    /// <summary>
    /// Sends a confirmation email to verify a user's request to change their email address.
    /// </summary>
    /// <param name="toEmail">The recipient's new email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="changeLink">The unique link the user must click to confirm the email change.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SendEmailChangeConfirmationAsync(
        string toEmail,
        string userName,
        string changeLink,
        CancellationToken cancellationToken = default)
    {
        var placeholders = new Dictionary<string, string>
        {
            { "USER_NAME", userName },
            { "CHANGE_LINK", changeLink },
        };

        var body = await templateProvider.GetEmailTemplateAsync("email_change_confirmation", placeholders);

        await sender.SendAsync(toEmail, "Confirm your new email address", body, cancellationToken);
    }
}
