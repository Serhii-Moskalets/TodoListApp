using FluentValidation;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Domain.Entities;
using FVResult = FluentValidation.Results.ValidationResult;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="RemoveTagFromTaskCommandHandler"/>.
/// Verifies the behavior of the handler for removing a tag from a task.
/// </summary>
public class RemoveTagFromTaskCommandHandlerTests
{
    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<RemoveTagFromTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RemoveTagFromTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult([new FluentValidation.Results.ValidationFailure("TaskId", "Task ID is required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new RemoveTagFromTaskCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task ID is required", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<RemoveTagFromTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RemoveTagFromTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new RemoveTagFromTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures the handler returns success when the task exists but has no tag.
    /// No changes are saved to the database in this case.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTaskHasNoTag()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<RemoveTagFromTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RemoveTagFromTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Title");

        task.SetTag(null);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new RemoveTagFromTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Ensures the handler removes the tag from the task and saves changes when the task has a tag.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldRemoveTag_WhenTaskHasTag()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<RemoveTagFromTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<RemoveTagFromTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Title");

        task.SetTag(Guid.NewGuid());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new RemoveTagFromTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new RemoveTagFromTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(task.TagId);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
