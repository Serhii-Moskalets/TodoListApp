using TodoListApp.Application.Tag.Commands.CreateTag;

namespace TodoListApp.Application.Tests.Tag.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTagCommandValidator"/>.
/// Tests validation rules for creating a tag.
/// </summary>
public class CreateTagCommandValidatorTests
{
    private readonly CreateTagCommandValidator _validator = new();

    /// <summary>
    /// Tests that validation fails when the tag name is empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsEmpty()
    {
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Name");
        Assert.Equal("Tag name cannot be null or empty.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when the tag name exceeds the maximum length (50 characters).
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenNameIsTooLong()
    {
        var longName = new string('a', 51);
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), longName);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Name");
        Assert.Equal("Tag name cannot exceed 50 characters.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the tag name is not empty and within length limit.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotHaveError_WhenNameIsValid()
    {
        var command = new CreateTagCommand(Guid.NewGuid(), Guid.NewGuid(), "Tag");

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
