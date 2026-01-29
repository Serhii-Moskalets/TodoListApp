using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents an application user with tasks, task lists, comments, and tags.
/// </summary>
[Table("users")]
public class UserEntity : BaseEntity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserEntity"/> class.
    /// </summary>
    /// <param name="firstName">The first name of the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="passwordHash">The password hash of the user.</param>
    /// <param name="lastName">The last name of the user (optional).</param>
    /// <exception cref="DomainException">
    /// Thrown when
    /// <paramref name="firstName"/>, <paramref name="userName"/>,
    /// <paramref name="email"/>, <paramref name="passwordHash"/>
    /// is null, empty, or consists only of white-space characters.
    /// </exception>
    public UserEntity(string firstName, string userName, string email, string passwordHash, string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new DomainException("First name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new DomainException("User name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Email cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException("Password hash cannot be empty.");
        }

        if ((lastName?.Trim())?.Length == 0)
        {
            lastName = null;
        }

        this.FirstName = firstName.Trim();
        this.LastName = lastName?.Trim();
        this.UserName = userName.Trim();
        this.Email = email.Trim();
        this.PasswordHash = passwordHash;
    }

    private UserEntity() { }

    /// <summary>
    /// Gets the first name of the user.
    /// </summary>
    [Column("first_name")]
    public string FirstName { get; private set; } = null!;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    [Column("last_name")]
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets the username of the user.
    /// </summary>
    [Column("user_name")]
    public string UserName { get; init; } = null!;

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Column("email")]
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Gets the pending email address for email change operations.
    /// </summary>
    [Column("pending_email")]
    public string? PendingEmail { get; private set; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    [Column("password_hash")]
    public string PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user's email is confirmed.
    /// </summary>
    [Column("email_confirmed")]
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Gets the token value for email verification, password reset, or email change.
    /// </summary>
    [Column("token_value")]
    public string? TokenValue { get; private set; }

    /// <summary>
    /// Gets the expiration date of the current token.
    /// </summary>
    [Column("token_expires")]
    public DateTime? TokenExpires { get; private set; }

    /// <summary>
    /// Gets the type of the current token.
    /// </summary>
    [Column("token_type")]
    public UserTokenType? TokenType { get; private set; }

    /// <summary>
    /// Gets the comments created by the user.
    /// </summary>
    public virtual ICollection<CommentEntity> Comments { get; init; } = [];

    /// <summary>
    /// Gets the tags owned by the user.
    /// </summary>
    public virtual ICollection<TagEntity> Tags { get; init; } = [];

    /// <summary>
    /// Gets the task lists owned by the user.
    /// </summary>
    public virtual ICollection<TaskListEntity> TaskLists { get; init; } = [];

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public virtual ICollection<TaskEntity> OwnedTasks { get; init; } = [];

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public virtual ICollection<UserTaskAccessEntity> TaskAccesses { get; init; } = [];

    /// <summary>
    /// Sets an email verification token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="token"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void SetEmailVerificationToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.EmailVerification);
    }

    /// <summary>
    /// Sets a password reset token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="token"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void SetPasswordResetToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.PasswordReset);
    }

    /// <summary>
    /// Sets an email change token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="token"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void SetEmailChangeToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new DomainException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.EmailChange);
    }

    /// <summary>
    /// Sets a pending email for email change operations.
    /// </summary>
    /// <param name="newEmail">The new pending email address.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newEmail"/> is null, empty, or consists only of white-space characters or
    /// new email queals old current email.
    /// </exception>
    public void SetPendingEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw new DomainException("New email cannot be empty.");
        }

        if (string.Equals(newEmail?.Trim(), this.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("New email cannot be the same as current email.");
        }

        this.EmailConfirmed = false;
        this.PendingEmail = newEmail!.Trim();
    }

    /// <summary>
    /// Confirms the email change using the pending email and token.
    /// </summary>
    public void ConfirmEmailChange()
    {
        this.ValidateToken(UserTokenType.EmailChange);

        if (string.IsNullOrWhiteSpace(this.PendingEmail))
        {
            throw new DomainException("Pending email is not set.");
        }

        this.Email = this.PendingEmail;
        this.EmailConfirmed = true;
        this.ClearPendingEmail();
        this.ClearToken();
    }

    /// <summary>
    /// Clears the pending email.
    /// </summary>
    public void ClearPendingEmail()
        => this.PendingEmail = null;

    /// <summary>
    /// Clears the current token values.
    /// </summary>
    public void ClearToken()
    {
        this.TokenValue = null;
        this.TokenExpires = null;
        this.TokenType = null;
    }

    /// <summary>
    /// Confirms the user's email and clears the token.
    /// </summary>
    public void ConfirmEmail()
    {
        this.ValidateToken(UserTokenType.EmailVerification);
        this.EmailConfirmed = true;
        this.ClearToken();
    }

    /// <summary>
    /// Sets the new password hash.
    /// </summary>
    /// <param name="newHash">The new password hash.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newHash"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void SetPasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
        {
            throw new DomainException("Password cannot be empty.");
        }

        this.PasswordHash = newHash;
    }

    /// <summary>
    /// Resets the user's password using a password reset token.
    /// </summary>
    /// <param name="newHash">The new password hash.</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newHash"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void ResetPassword(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
        {
            throw new DomainException("Password cannot be empty.");
        }

        this.ValidateToken(UserTokenType.PasswordReset);
        this.PasswordHash = newHash;
        this.ClearToken();
    }

    /// <summary>
    /// Updates the first and last name of the user.
    /// </summary>
    /// <param name="newFirstName">The new first name.</param>
    /// <param name="newLastName">The new last name (optional).</param>
    /// <exception cref="DomainException">
    /// Thrown when <paramref name="newFirstName"/> is null, empty, or consists only of white-space characters.
    /// </exception>
    public void UpdateFirstAndLastName(string newFirstName, string? newLastName = null)
    {
        if (string.IsNullOrWhiteSpace(newFirstName))
        {
            throw new DomainException("First name cannot be empty.");
        }

        this.FirstName = newFirstName.Trim();
        this.LastName = string.IsNullOrWhiteSpace(newLastName?.Trim())
            ? null
            : newLastName.Trim();
    }

    /// <summary>
    /// Sets the token value, expiration, and type.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    /// <param name="tokenType">The type of the token.</param>
    private void SetToken(string token, DateTime expires, UserTokenType tokenType)
    {
        this.TokenValue = token;
        this.TokenExpires = expires;
        this.TokenType = tokenType;
    }

    /// <summary>
    /// Validates the current token against the expected type and expiration.
    /// </summary>
    /// <param name="expectedType">The expected <see cref="UserTokenType"/> of the token.</param>
    /// <exception cref="DomainException">
    /// Thrown if the token type does not match <paramref name="expectedType"/>
    /// or if the token has expired.
    /// </exception>
    private void ValidateToken(UserTokenType expectedType)
    {
        if (this.TokenType != expectedType)
        {
            throw new DomainException($"Invalid token type. Expected {expectedType}.");
        }

        if (this.TokenExpires is null || this.TokenExpires < DateTime.UtcNow)
        {
            throw new DomainException("Token has expired.");
        }
    }
}
