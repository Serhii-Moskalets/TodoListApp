using FluentValidation;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Entities;
using FVResult = FluentValidation.Results.ValidationResult;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteOverdueTasksCommandHandlerTests
{
    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteOverdueTasksCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteOverdueTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVResult([new FluentValidation.Results.ValidationFailure("TaskListId", "Validation failed")]));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new DeleteOverdueTasksCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteOverdueTasksCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed", result.Error!.Message);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteOverdueTasksCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteOverdueTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVResult());

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new DeleteOverdueTasksCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteOverdueTasksCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures the handler deletes overdue tasks when the task list exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteOverdueTasks_WhenTaskListExists()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteOverdueTasksCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteOverdueTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVResult());

        var taskList = new Mock<TaskListEntity>(Guid.NewGuid(), "My list") { CallBase = true };
        taskList.Setup(t => t.DeleteOverdueTasks(It.IsAny<DateTime>()));

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList.Object);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteOverdueTasksCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteOverdueTasksCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        taskList.Verify(t => t.DeleteOverdueTasks(It.IsAny<DateTime>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
