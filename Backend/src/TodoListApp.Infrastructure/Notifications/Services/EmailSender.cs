using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TodoListApp.Infrastructure.Notifications.Settings;

namespace TodoListApp.Infrastructure.Notifications.Services;

/// <summary>
/// Provides low-level functionality to send emails using the SMTP protocol.
/// </summary>
/// <remarks>
/// This service uses MailKit to handle the actual delivery of messages.
/// </remarks>
/// <param name="settings">The SMTP configuration settings wrapped in IOptions.</param>
public class EmailSender(IOptions<EmailSettings> settings)
{
    private readonly EmailSettings _settings = settings.Value;

    /// <summary>
    /// Asynchronously sends an email message.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="subject">The email subject line.</param>
    /// <param name="body">The HTML body of the email.</param>
    /// <param name="ct">A token to monitor for cancellation requests.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the email delivery fails. This can be due to invalid email format,
    /// SMTP authentication failure, or network connectivity issues.
    /// </exception>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken ct)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(this._settings.FromName, this._settings.FromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();

            client.Timeout = 10000;

            await client.ConnectAsync(this._settings.Host, this._settings.Port, SecureSocketOptions.Auto, ct);
            await client.AuthenticateAsync(this._settings.UserName, this._settings.Password, ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send email to {toEmail}", ex);
        }
    }
}
