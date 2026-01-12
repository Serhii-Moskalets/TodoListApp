using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskListCommandHandler"/>.
/// Verifies validation, existence, and deletion behavior for task lists.
/// </summary>
public class DeleteTaskListCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("TaskListId", "Invalid ID") }));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new DeleteTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid ID", result.Error.Message);
    }

    /// <summary>
    /// Returns failure if the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((TaskListEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new DeleteTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Task list not found.", result.Error!.Message);
    }

    /// <summary>
    /// Deletes the task list when validation passes and it exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTaskList_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<DeleteTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var taskList = new Domain.Entities.TaskListEntity(Guid.NewGuid(), "Test TaskList");

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(taskList);
        taskListRepoMock.Setup(r => r.DeleteAsync(taskList, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskListCommand(taskList.Id, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        taskListRepoMock.Verify(r => r.DeleteAsync(taskList, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
