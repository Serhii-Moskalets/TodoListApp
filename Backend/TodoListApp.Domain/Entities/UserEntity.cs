using System.ComponentModel.DataAnnotations;
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
    /// <param name="lastName">The last name of the user.</param>
    /// <param name="userName">The username of the user.</param>
    /// <param name="email">The email of the user.</param>
    /// <param name="passwordHash">The pasword hash of the user.</param>
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
    required public string FirstName { get; init; }

    /// <summary>
    /// Gets the last name of the user.
    /// </summary>
    [Column("Last_Name")]
    public string? LastName { get; init; }

    /// <summary>
    /// Gets he username of the user.
    /// </summary>
    [Column("User_Name")]
    required public string UserName { get; init; }

    /// <summary>
    /// Gets the email address of the user.
    /// </summary>
    [Column("Email")]
    required public string Email { get; init; }

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
    public virtual ICollection<CommentEntity> Comments { get; private set; } = new HashSet<CommentEntity>();

    /// <summary>
    /// Gets the tags owned by the user.
    /// </summary>
    public virtual ICollection<TagEntity> Tags { get; private set; } = new HashSet<TagEntity>();

    /// <summary>
    /// Gets the task lists owned by the user.
    /// </summary>
    public virtual ICollection<TaskListEntity> TaskLists { get; private set; } = new HashSet<TaskListEntity>();

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public virtual ICollection<TaskEntity> OwnedTasks { get; private set; } = new HashSet<TaskEntity>();

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public virtual ICollection<UserTaskAccessEntity> TaskAccesses { get; private set; } = new HashSet<UserTaskAccessEntity>();

    /// <summary>
    /// Sets an email verification token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailVerificationToken(string token, DateTime expires) =>
        this.SetToken(token, expires, UserTokenType.EmailVerification);

    /// <summary>
    /// Sets a password reset token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetPasswordResetToken(string token, DateTime expires) =>
        this.SetToken(token, expires, UserTokenType.PasswordReset);

    /// <summary>
    /// Sets an email change token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailChangeToken(string token, DateTime expires) =>
        this.SetToken(token, expires, UserTokenType.EmailChange);

    /// <summary>
    /// Sets a pending email for email change operations.
    /// </summary>
    /// <param name="newEmail">The new pending email address.</param>
    public void SetPendingEmail(string newEmail) => this.PendingEmail = newEmail;

    /// <summary>
    /// Clears the pending email.
    /// </summary>
    public void ClearPendingEmail() => this.PendingEmail = null;

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
        this.EmailConfirmed = true;
        this.ClearToken();
    }

    /// <summary>
    /// Sets the new password hash.
    /// </summary>
    /// <param name="newHash">The new password hash.</param>
    public void SetPasswordHash(string newHash)
    {
        this.PasswordHash = newHash;
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
}
