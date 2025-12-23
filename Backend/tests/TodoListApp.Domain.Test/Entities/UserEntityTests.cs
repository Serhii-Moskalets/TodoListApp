using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Domain.Test.Entities;

/// <summary>
/// Unit tests for the <see cref="UserEntity"/> class.
/// </summary>
public class UserEntityTests
{
    private const string FirstName = "John";
    private const string LastName = "Doe";
    private const string UserName = "jdoe";
    private const string Email = "john@example.com";
    private const string PasswordHash = "hashedPassword";
    private const string TokenValue = "token123";
    private static readonly DateTime TokenExpires = DateTime.UtcNow.AddHours(1);

    /// <summary>
    /// Tests that the constructor creates a user with valid data.
    /// </summary>
    [Fact]
    public void Constructor_Should_CreateUser_When_ValidData()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash, LastName);

        Assert.Equal(FirstName, user.FirstName);
        Assert.Equal(LastName, user.LastName);
        Assert.Equal(UserName, user.UserName);
        Assert.Equal(Email, user.Email);
        Assert.Equal(PasswordHash, user.PasswordHash);
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentException"/>
    /// when the first name is null, empty, or whitespace.
    /// </summary>
    /// <param name="firstName">The first name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_FirstNameIsInvalid(string? firstName)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserEntity(firstName!, UserName, Email, PasswordHash));
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentException"/>
    /// when the user name is null, empty, or whitespace.
    /// </summary>
    /// <param name="userName">The user name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_UserNameIsInvalid(string? userName)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserEntity(FirstName, userName!, Email, PasswordHash));
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentException"/>
    /// when the email is null, empty, or whitespace.
    /// </summary>
    /// <param name="email">The email to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_EmailIsInvalid(string? email)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserEntity(FirstName, UserName, email!, PasswordHash));
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetEmailVerificationToken"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void SetEmailVerificationToken_Should_SetToken()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.SetEmailVerificationToken(TokenValue, TokenExpires);

        Assert.Equal(TokenValue, user.TokenValue);
        Assert.Equal(TokenExpires, user.TokenExpires);
        Assert.Equal(UserTokenType.EmailVerification, user.TokenType);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetPasswordResetToken"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void SetPasswordResetToken_Should_SetToken()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.SetPasswordResetToken(TokenValue, TokenExpires);

        Assert.Equal(TokenValue, user.TokenValue);
        Assert.Equal(TokenExpires, user.TokenExpires);
        Assert.Equal(UserTokenType.PasswordReset, user.TokenType);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetEmailChangeToken"/> sets token value, expiry, and type correctly.
    /// </summary>
    [Fact]
    public void EmailChangeToken_Should_SetToken()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.SetEmailChangeToken(TokenValue, TokenExpires);

        Assert.Equal(TokenValue, user.TokenValue);
        Assert.Equal(TokenExpires, user.TokenExpires);
        Assert.Equal(UserTokenType.EmailChange, user.TokenType);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetPendingEmail"/> sets a pending email
    /// and unsets the <see cref="UserEntity.EmailConfirmed"/> flag.
    /// </summary>
    [Fact]
    public void SetPendingEmail_Should_SetPendingEmailAndUnsetConfirmed()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.SetPendingEmail("new@example.com");

        Assert.Equal("new@example.com", user.PendingEmail);
        Assert.False(user.EmailConfirmed);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ConfirmEmailChange"/> updates the email,
    /// confirms it, clears pending email and token.
    /// </summary>
    [Fact]
    public void ConfirmEmailChange_Should_UpdateEmailAndClearToken()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.SetEmailChangeToken(TokenValue, TokenExpires);
        user.SetPendingEmail("new@example.com");

        user.ConfirmEmailChange();

        Assert.Equal("new@example.com", user.Email);
        Assert.True(user.EmailConfirmed);
        Assert.Null(user.PendingEmail);
        Assert.Null(user.TokenValue);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetPasswordHash"/> updates the password hash.
    /// </summary>
    [Fact]
    public void SetPasswordHash_Should_UpdatePassword()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetPasswordHash("newHash");
        Assert.Equal("newHash", user.PasswordHash);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ResetPassword"/> updates the password hash
    /// when a valid password reset token is present.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_UpdatePassword_When_ValidToken()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetPasswordResetToken(TokenValue, TokenExpires);

        user.ResetPassword("newHash");

        Assert.Equal("newHash", user.PasswordHash);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.UpdateFirstAndLastName"/> updates first and last names.
    /// </summary>
    [Fact]
    public void UpdateFirstAndLastName_Should_UpdateNames()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);

        user.UpdateFirstAndLastName("Jane", "Smith");

        Assert.Equal("Jane", user.FirstName);
        Assert.Equal("Smith", user.LastName);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.UpdateFirstAndLastName"/> throws <see cref="ArgumentException"/>
    /// when the first name is null, empty, or whitespace.
    /// </summary>
    /// <param name="newFirstName">The new first name to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void UpdateFirst_ShouldThrow_InvalidFirstName(string? newFirstName)
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        Assert.Throws<ArgumentException>(() => user.UpdateFirstAndLastName(newFirstName!));
    }

    /// <summary>
    /// Tests that the <see cref="UserEntity"/> constructor throws an <see cref="ArgumentException"/>
    /// when the password hash is null, empty, or whitespace.
    /// </summary>
    /// <param name="passwordHash">The password hash to test.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_Should_Throw_When_PasswordHashIsInvalid(string? passwordHash)
    {
        Assert.Throws<ArgumentException>(() =>
            new UserEntity(FirstName, UserName, Email, passwordHash!));
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ResetPassword"/> throws <see cref="InvalidOperationException"/>
    /// when the token type is invalid.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_Throw_When_TokenTypeIsInvalid()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetEmailVerificationToken(TokenValue, TokenExpires);

        Assert.Throws<InvalidOperationException>(() => user.ResetPassword("newHash"));
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ResetPassword"/> throws <see cref="InvalidOperationException"/>
    /// when the password reset token has expired.
    /// </summary>
    [Fact]
    public void ResetPassword_Should_Throw_When_TokenExpired()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetPasswordResetToken(TokenValue, DateTime.UtcNow.AddMinutes(-1));

        Assert.Throws<InvalidOperationException>(() => user.ResetPassword("newHash"));
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.SetPendingEmail"/> throws <see cref="InvalidOperationException"/>
    /// when the pending email is the same as the current email.
    /// </summary>
    [Fact]
    public void SetPendingEmail_Should_Throw_When_EmailSameAsCurrent()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        Assert.Throws<InvalidOperationException>(() => user.SetPendingEmail(Email));
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.UpdateFirstAndLastName"/> sets last name to null
    /// when the new last name is empty or whitespace.
    /// </summary>
    [Fact]
    public void UpdateFirstAndLastName_Should_SetLastNameToNull_When_Empty()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.UpdateFirstAndLastName("Jane", " ");

        Assert.Null(user.LastName);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ClearToken"/> clears all token-related fields.
    /// </summary>
    [Fact]
    public void ClearToken_Should_SetTokenNull()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetEmailVerificationToken(TokenValue, TokenExpires);
        user.ClearToken();
        Assert.Null(user.TokenExpires);
        Assert.Null(user.TokenValue);
        Assert.Null(user.TokenType);
    }

    /// <summary>
    /// Tests that <see cref="UserEntity.ClearPendingEmail"/> clears the pending email.
    /// </summary>
    [Fact]
    public void ClearPendingEmail_Should_PendingEmailNull()
    {
        var user = new UserEntity(FirstName, UserName, Email, PasswordHash);
        user.SetPendingEmail("jane@gmail.com");
        user.ClearPendingEmail();
        Assert.Null(user.PendingEmail);
    }
}
