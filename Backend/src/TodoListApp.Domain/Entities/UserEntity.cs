using System.ComponentModel.DataAnnotations.Schema;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents an application user with tasks, task lists, comments, and tags.
/// </summary>
[Table("Users")]
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
    public UserEntity(string firstName, string userName, string email, string passwordHash, string? lastName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        }

        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name cannot be empty.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));
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
    [Column("First_Name")]
    public string FirstName { get; private set; } = null!;

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    [Column("Last_Name")]
    public string? LastName { get; private set; }

    /// <summary>
    /// Gets he username of the user.
    /// </summary>
    [Column("User_Name")]
    public string UserName { get; private set; } = null!;

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Column("Email")]
    public string Email { get; private set; } = null!;

    /// <summary>
    /// Gets the pending email address for email change operations.
    /// </summary>
    [Column("Pending_Email")]
    public string? PendingEmail { get; private set; }

    /// <summary>
    /// Gets the hashed password of the user.
    /// </summary>
    [Column("Password_Hash")]
    public string PasswordHash { get; private set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user's email is confirmed.
    /// </summary>
    [Column("Email_Confirmed")]
    public bool EmailConfirmed { get; private set; }

    /// <summary>
    /// Gets the token value for email verification, password reset, or email change.
    /// </summary>
    [Column("Token_Value")]
    public string? TokenValue { get; private set; }

    /// <summary>
    /// Gets the expiration date of the current token.
    /// </summary>
    [Column("Token_Expires")]
    public DateTime? TokenExpires { get; private set; }

    /// <summary>
    /// Gets the type of the current token.
    /// </summary>
    [Column("Token_Type")]
    public UserTokenType? TokenType { get; private set; }

    /// <summary>
    /// Gets the comments created by the user.
    /// </summary>
    public virtual ICollection<CommentEntity> Comments { get; init; } = new HashSet<CommentEntity>();

    /// <summary>
    /// Gets the tags owned by the user.
    /// </summary>
    public virtual ICollection<TagEntity> Tags { get; init; } = new HashSet<TagEntity>();

    /// <summary>
    /// Gets the task lists owned by the user.
    /// </summary>
    public virtual ICollection<TaskListEntity> TaskLists { get; init; } = new HashSet<TaskListEntity>();

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public virtual ICollection<TaskEntity> OwnedTasks { get; init; } = new HashSet<TaskEntity>();

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public virtual ICollection<UserTaskAccessEntity> TaskAccesses { get; init; } = new HashSet<UserTaskAccessEntity>();

    /// <summary>
    /// Sets an email verification token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailVerificationToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.EmailVerification);
    }

    /// <summary>
    /// Sets a password reset token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetPasswordResetToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.PasswordReset);
    }

    /// <summary>
    /// Sets an email change token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailChangeToken(string token, DateTime expires)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be empty.");
        }

        this.SetToken(token, expires, UserTokenType.EmailChange);
    }

    /// <summary>
    /// Sets a pending email for email change operations.
    /// </summary>
    /// <param name="newEmail">The new pending email address.</param>
    public void SetPendingEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
        {
            throw new ArgumentException("New email cannot be empty.", nameof(newEmail));
        }

        if (string.Equals(newEmail?.Trim(), this.Email, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("New email cannot be the same as current email.");
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
            throw new InvalidOperationException("Pending email is not set.");
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
    public void SetPasswordHash(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(newHash));
        }

        this.PasswordHash = newHash;
    }

    /// <summary>
    /// Resets the user's password using a password reset token.
    /// </summary>
    /// <param name="newHash">The new password hash.</param>
    public void ResetPassword(string newHash)
    {
        if (string.IsNullOrWhiteSpace(newHash))
        {
            throw new ArgumentException("Password cannot be empty.", nameof(newHash));
        }

        this.ValidateToken(UserTokenType.PasswordReset);
        this.PasswordHash = newHash;
    }

    /// <summary>
    /// Updates the first and last name of the user.
    /// </summary>
    /// <param name="newFirstName">The new first name.</param>
    /// <param name="newLastName">The new last name (optional).</param>
    public void UpdateFirstAndLastName(string newFirstName, string? newLastName = null)
    {
        if (string.IsNullOrWhiteSpace(newFirstName.Trim()))
        {
            throw new ArgumentException("First name cannot be empty.", nameof(newFirstName));
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

    private void ValidateToken(UserTokenType expectedType)
    {
        if (this.TokenType != expectedType)
        {
            throw new InvalidOperationException($"Invalid token type. Expected {expectedType}.");
        }

        if (this.TokenExpires is null || this.TokenExpires < DateTime.UtcNow)
        {
            throw new InvalidOperationException("Token has expired.");
        }
    }
}
