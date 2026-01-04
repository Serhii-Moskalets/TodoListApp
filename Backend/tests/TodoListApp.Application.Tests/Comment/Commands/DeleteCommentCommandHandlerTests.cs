using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.DeleteComment;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteCommentCommandHandler"/>.
/// Verifies the behavior of the handler for deleting comments.
/// </summary>
public class DeleteCommentCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("CommentId", "Invalid comment id")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteCommentCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid comment id", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler deletes the comment successfully when validation passes.
    /// Ensures that DeleteAsync and SaveChangesAsync are called exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteComment_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var commentRepoMock = new Mock<ICommentRepository>();
        commentRepoMock
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var handler = new DeleteCommentCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var commentId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteCommentCommand(taskId, commentId, Guid.NewGuid());

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        commentRepoMock.Verify(
            r => r.DeleteAsync(commentId, It.IsAny<CancellationToken>()),
            Times.Once);

        uowMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
