namespace TodoListApp.Application.Abstractions.Interfaces.Notifications;

/// <summary>
/// Provides methods for sending email notifications.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a confirmation email to a newly registered user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="userName">The name of the user for the greeting.</param>
    /// <param name="confirmLink">The unique verification link.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendConfirmationEmailAsync(
        string toEmail,
        string userName,
        string confirmLink,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a confirmation email to verify a user's request to change their email address.
    /// </summary>
    /// <param name="toEmail">The recipient's new email address.</param>
    /// <param name="userName">The name of the user for personalization in the email.</param>
    /// <param name="changeLink">The unique link the user must click to confirm the email change.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendEmailChangeConfirmationAsync(
        string toEmail,
        string userName,
        string changeLink,
        CancellationToken cancellationToken = default);
}
