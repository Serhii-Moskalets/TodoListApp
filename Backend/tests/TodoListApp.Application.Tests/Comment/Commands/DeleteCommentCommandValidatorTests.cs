using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.DeleteComment;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteCommentCommandValidator"/>.
/// Verifies validation rules for deleting a comment.
/// </summary>
public class DeleteCommentCommandValidatorTests
{
    /// <summary>
    /// Tests that validation passes when the user owns the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldPass_WhenUserOwnsComment()
    {
        var commentId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new DeleteCommentCommandValidator(unitOfWorkMock.Object);
        var command = new DeleteCommentCommand(taskId, commentId, userId);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    /// <summary>
    /// Tests that validation fails when the user does not own the comment.
    /// Ensures the proper error message is returned.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldFail_WhenUserDoesNotOwnComment()
    {
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        unitOfWorkMock
            .Setup(u => u.Comments.ExistsInTaskAndOwnedByUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new DeleteCommentCommandValidator(unitOfWorkMock.Object);
        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "CommentId");
        Assert.Equal(
            "Comment not found or does not belong to the user.",
            error.ErrorMessage);
    }
}
