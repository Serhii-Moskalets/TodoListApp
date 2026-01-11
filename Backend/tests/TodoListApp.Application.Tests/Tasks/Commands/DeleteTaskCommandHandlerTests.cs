using FluentValidation;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteTask;
using TodoListApp.Domain.Entities;
using FVResult = FluentValidation.Results.ValidationResult;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskCommandHandler"/>.
/// Verifies the behavior of the handler for deleting a task.
/// </summary>
public class DeleteTaskCommandHandlerTests
{
    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult([new FluentValidation.Results.ValidationFailure("TaskId", "Task ID is required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new DeleteTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskCommand(Guid.Empty, Guid.NewGuid());

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
        var validatorMock = new Mock<IValidator<DeleteTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new DeleteTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures the handler deletes the task when it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTask_WhenTaskExists()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var taskId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, taskListId, "Sample task");

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, ownerId, false, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(task);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteTaskCommand(taskId, ownerId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        taskRepoMock.Verify(r => r.DeleteAsync(task, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
