using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.DeleteComment;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteCommentCommandHandler"/>.
/// Verifies validation, existence, and permission checks when deleting comments.
/// </summary>
public class DeleteCommentCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("CommentId", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteCommentCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Required", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if the comment does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenCommentNotFound()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
               .ReturnsAsync((CommentEntity?)null);

        var handler = new DeleteCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Comment not found.", result.Error.Message);
    }

    /// <summary>
    /// Returns failure if the user is not the owner of the task and cannot delete the comment.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserHasNoPermission()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var comment = new CommentEntity(Guid.NewGuid(), Guid.NewGuid(), "Test");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(It.IsAny<Guid>(), true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        uowMock.Setup(u => u.Tasks.IsTaskOwnerAsync(comment.TaskId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var handler = new DeleteCommentCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteCommentCommand(comment.Id, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("You don't have permission to delete this comment.", result.Error.Message);
    }

    /// <summary>
    /// Deletes the comment successfully when the user is the comment owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldDeleteComment_WhenUserIsCommentOwner()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var comment = new CommentEntity(taskId, userId, "Test");

        var commentsRepoMock = new Mock<ICommentRepository>();
        commentsRepoMock.Setup(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        commentsRepoMock.Setup(r => r.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(comment);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(t => t.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(comment);
        uowMock.Setup(u => u.Comments).Returns(commentsRepoMock.Object);
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteCommentCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteCommentCommand(comment.Id, userId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        commentsRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Deletes the comment successfully when the user is the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldDeleteComment_WhenUserIsTaskOwner()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var commentOwnerId = Guid.NewGuid();
        var taskOwnerId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var comment = new CommentEntity(taskId, commentOwnerId, "Test");

        var commentsRepoMock = new Mock<ICommentRepository>();
        commentsRepoMock.Setup(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        commentsRepoMock.Setup(r => r.GetByIdAsync(comment.Id, true, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(comment);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(t => t.IsTaskOwnerAsync(taskId, taskOwnerId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentsRepoMock.Object);
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteCommentCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteCommentCommand(comment.Id, taskOwnerId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        commentsRepoMock.Verify(r => r.DeleteAsync(comment, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
