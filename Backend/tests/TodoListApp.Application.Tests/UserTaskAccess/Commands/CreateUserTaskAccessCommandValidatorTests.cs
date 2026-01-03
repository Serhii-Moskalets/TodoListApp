using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="CreateUserTaskAccessCommandValidator"/>.
/// Tests the validation rules for creating user-task access,
/// focusing on email requirements and formatting.
/// </summary>
public class CreateUserTaskAccessCommandValidatorTests
{
    private readonly CreateUserTaskAccessCommandValidator _validator = new();

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
        var createCommand = new CreateUserTaskAccessCommand(Guid.NewGuid(), email);

        var result = this._validator.Validate(createCommand);

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
        var createCommand = new CreateUserTaskAccessCommand(Guid.NewGuid(), email);

        var result = this._validator.Validate(createCommand);

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
        var createCommand = new CreateUserTaskAccessCommand(Guid.NewGuid(), "test@test.com");

        var result = this._validator.Validate(createCommand);

        Assert.True(result.IsValid);
    }
}
