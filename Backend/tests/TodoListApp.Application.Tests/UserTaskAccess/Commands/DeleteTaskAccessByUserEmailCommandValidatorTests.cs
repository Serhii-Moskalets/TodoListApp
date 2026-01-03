using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByUserEmailCommandValidator"/>.
/// Tests the validation rules for deleting user-task access,
/// focusing on email requirements and formatting.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandValidatorTests
{
    private readonly DeleteTaskAccessByUserEmailCommandValidator _validator = new();

    /// <summary>
    /// Ensures that the validator produces an error when the email is null, empty, or whitespace.
    /// </summary>
    /// <param name="email">The email value to test.</param>
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData(" ")]
    public void Validator_ShouldHaveError_WhenEmailIsNullOrEmpty(string? email)
    {
        var deleteCommand = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), email);

        var result = this._validator.Validate(deleteCommand);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Email");
        Assert.Equal("Email cannot be null or empty.", error.ErrorMessage);
    }

    /// <summary>
    /// Ensures that the validator produces an error when the email format is invalid.
    /// </summary>
    /// <param name="email">The email value to test.</param>
    [Theory]
    [InlineData("sometext")]
    [InlineData("test@")]
    [InlineData("@gsss")]
    public void Validator_ShouldHaveError_WhenEmailIsInvalid(string? email)
    {
        var deleteCommand = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), email);

        var result = this._validator.Validate(deleteCommand);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Email");
        Assert.Equal("Email address is incorrect.", error.ErrorMessage);
    }

    /// <summary>
    /// Ensures that the validator passes when a valid email is provided.
    /// </summary>
    [Fact]
    public void Validator_ShouldNotHaveError_WhenEmailIsValid()
    {
        var deleteCommand = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com");

        var result = this._validator.Validate(deleteCommand);

        Assert.True(result.IsValid);
    }
}
