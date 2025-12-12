using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoListApp.Domain.Entities;

/// <summary>
/// Represents an application user with tasks, task lists, comments, and tags.
/// </summary>
[Table("Users")]
public class UserEntity
{
    /// <summary>
    /// Gets the unique identifier of the user.
    /// </summary>
    [Column("User_Id")]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the first name of the user.
    /// </summary>
    [Column("First_Name")]
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the last name of the user.
    /// </summary>
    [Column("Last_Name")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the username of the user.
    /// </summary>
    [Required]
    [Column("User_Name")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    [Column("Email")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets the pending email address for email change operations.
    /// </summary>
    [Column("Pending_Email")]
    public string? PendingEmail { get; private set; }

    /// <summary>
    /// Gets or sets the hashed password of the user.
    /// </summary>
    [Column("Password_Hash")]
    public string PasswordHash { get; set; } = null!;

    /// <summary>
    /// Gets a value indicating whether the user's email is confirmed.
    /// </summary>
    [Column("Email_Confirmed")]
    public bool EmailConfirmed { get; private set; } = false;

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
    public string? TokenType { get; private set; }

    /// <summary>
    /// Gets the comments created by the user.
    /// </summary>
    public List<CommentEntity> Comments { get; private set; } = new();

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public List<TagEntity> Tags { get; private set; } = new();

    /// <summary>
    /// Gets the task lists owned by the user.
    /// </summary>
    public List<TaskListEntity> TaskLists { get; private set; } = new();

    /// <summary>
    /// Gets the tasks owned by the user.
    /// </summary>
    public List<TaskEntity> OwnedTasks { get; private set; } = new();

    /// <summary>
    /// Gets the task access records for the user.
    /// </summary>
    public List<UserTaskAccessEntity> TaskAccesses { get; private set; } = new();

    /// <summary>
    /// Sets an email verification token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailVerificationToken(string token, DateTime expires) =>
        this.SetToken(token, expires, "EmailVerification");

    /// <summary>
    /// Sets a password reset token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetPasswordResetToken(string token, DateTime expires) =>
        this.SetToken(token, expires, "PasswordReset");

    /// <summary>
    /// Sets an email change token for the user.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    public void SetEmailChangeToken(string token, DateTime expires) =>
        this.SetToken(token, expires, "EmailChange");

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
    /// Sets the token value, expiration, and type.
    /// </summary>
    /// <param name="token">The token value to set.</param>
    /// <param name="expires">The expiration date and time of the token.</param>
    /// <param name="tokenType">The type of the token.</param>
    private void SetToken(string token, DateTime expires, string tokenType)
    {
        this.TokenValue = token;
        this.TokenExpires = expires;
        this.TokenType = tokenType;
    }
}
