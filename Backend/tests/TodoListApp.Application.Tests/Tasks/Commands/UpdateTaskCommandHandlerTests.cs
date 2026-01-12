using FluentValidation;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;
using FVResult = FluentValidation.Results.ValidationResult;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskCommandHandler"/>.
/// Ensures that the handler correctly validates, finds, and updates a task.
/// </summary>
public class UpdateTaskCommandHandlerTests
{
    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVResult([new FluentValidation.Results.ValidationFailure("TaskId", "Task ID is required.")]));

        var uowMock = new Mock<IUnitOfWork>();
        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var updateTaskDto = new UpdateTaskDto
        {
            TaskId = Guid.Empty,
            Title = "New Title",
        };

        var command = new UpdateTaskCommand(updateTaskDto, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task ID is required.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a not-found error when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FVResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var updateTaskDto = new UpdateTaskDto
        {
            TaskId = Guid.NewGuid(),
            Title = "New Title",
        };

        var command = new UpdateTaskCommand(updateTaskDto, Guid.NewGuid());

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler updates the task and saves changes when the command is valid.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTask_WhenCommandIsValid()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task = new TaskEntity(userId, taskListId, "Old Title");

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(task.Id, userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var updateTaskDto = new UpdateTaskDto
        {
            TaskId = task.Id,
            Title = "New Title",
            Description = "New Description",
            DueDate = DateTime.UtcNow.AddDays(1),
        };

        var command = new UpdateTaskCommand(updateTaskDto, userId);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("New Title", task.Title);
        Assert.Equal("New Description", task.Description);
        Assert.Equal(updateTaskDto.DueDate, task.DueDate);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
