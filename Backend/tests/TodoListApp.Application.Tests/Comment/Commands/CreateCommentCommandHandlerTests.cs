using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Comment.Commands.CreateComment;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Comment.Commands;

/// <summary>
/// Unit tests for <see cref="CreateCommentCommandHandler"/>.
/// Verifies validation, access checks, and comment creation.
/// </summary>
public class CreateCommentCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateCommentCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult([new ValidationFailure("Text", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var taskAccessMock = new Mock<ITaskAccessService>();

        var handler = new CreateCommentCommandHandler(uowMock.Object, taskAccessMock.Object, validatorMock.Object);
        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Required", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldReturnFailure_WhenUserHasNoAccess()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var uowMock = new Mock<IUnitOfWork>();
        var taskAccessMock = new Mock<ITaskAccessService>();
        taskAccessMock
            .Setup(s => s.HasAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateCommentCommandHandler(uowMock.Object, taskAccessMock.Object, validatorMock.Object);

        var command = new CreateCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Text");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("You don't have access to this task.", result.Error!.Message);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
    }

    /// <summary>
    /// Adds a comment successfully when validation passes and user has access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HandleAsync_ShouldAddComment_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateCommentCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateCommentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var commentsRepoMock = new Mock<ICommentRepository>();
        commentsRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CommentEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Comments).Returns(commentsRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var taskAccessMock = new Mock<ITaskAccessService>();
        taskAccessMock
            .Setup(s => s.HasAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateCommentCommandHandler(uowMock.Object, taskAccessMock.Object, validatorMock.Object);

        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateCommentCommand(taskId, userId, "Valid comment");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        commentsRepoMock
            .Verify(
            r =>
            r.AddAsync(
                It.Is<CommentEntity>(c =>
                    c.TaskId == taskId &&
                    c.UserId == userId &&
                    c.Text == "Valid comment"),
                It.IsAny<CancellationToken>()), Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
