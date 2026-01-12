using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.CreateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskCommandHandler"/>.
/// Verifies handler behavior for validation, not-found scenarios, and successful task creation.
/// </summary>
public class CreateTaskCommandHandlerTests
{
    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateTaskCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([
                new ValidationFailure("Title", "Title is required")
            ]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateTaskCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var createTaskDto = new CreateTaskDto
        {
            DueDate = DateTime.Now.AddDays(1),
            TaskListId = Guid.NewGuid(),
            Title = string.Empty,
        };

        var command = new CreateTaskCommand(createTaskDto, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Title is required", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateTaskCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new CreateTaskCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var createTaskDto = new CreateTaskDto
        {
            DueDate = DateTime.Now.AddDays(1),
            TaskListId = Guid.NewGuid(),
            Title = "Title",
        };

        var command = new CreateTaskCommand(createTaskDto, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures the handler creates a task when the command is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateTask_WhenCommandIsValid()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateTaskCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var taskList = new TaskListEntity(userId, "My list");

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock
            .Setup(r => r.GetTaskListByIdForUserAsync(
                taskListId,
                userId,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskList);

        var taskRepoMock = new Mock<ITaskRepository>();

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var handler = new CreateTaskCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var createTaskDto = new CreateTaskDto
        {
            DueDate = DateTime.UtcNow.AddDays(1),
            TaskListId = taskListId,
            Title = "New task",
        };

        var command = new CreateTaskCommand(createTaskDto, userId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);

        taskRepoMock.Verify(
            r =>
            r.AddAsync(
                It.Is<TaskEntity>(t =>
                t.OwnerId == userId &&
                t.TaskListId == taskListId &&
                t.Title == "New task"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
