using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.UpdateComment;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateCommentCommandValidator"/>.
/// Ensures that the validator correctly enforces rules for updating a comment.
/// </summary>
public class UpdateCommentCommandValidatorTests
{
    /// <summary>
    /// Tests that validation returns an error when the user does not own the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserDoesNotOwnComment()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new UpdateCommentCommandValidator(unitOfWorkMock.Object);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Some text");

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "CommentId");
        Assert.Equal("Comment not found or does not belong to the user.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the user owns the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserOwnsComment()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new UpdateCommentCommandValidator(unitOfWorkMock.Object);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Some text");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation returns an error when <see cref="UpdateCommentCommand.NewText"/> is empty.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenNewTextIsEmpty()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new UpdateCommentCommandValidator(unitOfWorkMock.Object);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "NewText");
        Assert.Equal("New text cannot be null or empty.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when <see cref="UpdateCommentCommand.NewText"/> is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveNotHaveError_WhenNewTextIsValid()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new UpdateCommentCommandValidator(unitOfWorkMock.Object);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Valid comment text");

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation returns an error when <see cref="UpdateCommentCommand.NewText"/> exceeds 1000 characters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenNewTextIsTooLong()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new UpdateCommentCommandValidator(unitOfWorkMock.Object);

        var longText = new string('a', 1001);
        var command = new UpdateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), longText);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "NewText");
        Assert.Equal("Comment text cannot exceed 1000 characters.", error.ErrorMessage);
    }
}
