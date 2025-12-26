using TodoListApp.Application.Comment.Commands.CreateComment;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandValidator"/>.
/// Tests validation rules for creating a tag.
/// </summary>
public class CreateCommentCommandValidatorTests
{
    private readonly CreateCommentCommandValidator _validator = new();

    /// <summary>
    /// Tests that validation fails when the comments text is empty.
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenTextIsEmpty()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Text");
        Assert.Equal("Comment text cannot be empty.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when the comments text exceeds the maximum length (1000 characters).
    /// </summary>
    [Fact]
    public void Validate_ShouldHaveError_WhenTextIsTooLong()
    {
        var text = new string('a', 1001);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), text);

        var result = this._validator.Validate(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Text");
        Assert.Equal("Comment text cannot exceed 1000 characters.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the tag name is not empty and within length limit.
    /// </summary>
    [Fact]
    public void Validate_ShouldNotHaveError_WhenTextIsValid()
    {
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Text");

        var result = this._validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
