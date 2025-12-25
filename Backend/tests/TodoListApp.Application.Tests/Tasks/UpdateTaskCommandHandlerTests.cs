using FluentValidation;
using Moq;
using TodoListApp.Application.Tasks.Commands.UpdateTask;
using TodoListApp.Application.Tasks.Dtos;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.Tasks;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskCommandHandler"/>.
/// Ensures that the command handler correctly validates, finds, and updates task.
/// </summary>
public class UpdateTaskCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("Title", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            TaskId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = string.Empty,
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler returns a failure result when the specified task is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            TaskId = Guid.NewGuid(),
            OwnerId = Guid.NewGuid(),
            Title = "New Task",
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler updates the task without description
    /// when validation passes and the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTask_WithoutDescription_WhenValidationPasses()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskEntity = new TaskEntity(userId, taskListId, "Old task");
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            TaskId = taskId,
            OwnerId = userId,
            Title = "New task",
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal("New task", taskEntity.Title);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that the handler updates the task with description and due date
    /// when validation passes and the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTask_WithDescriptionAndDueDate_WhenValidationPasses()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(1);

        var validatorMock = new Mock<IValidator<UpdateTaskCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskEntity = new TaskEntity(userId, taskListId, "Old task");
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateTaskCommand(new UpdateTaskDto
        {
            TaskId = taskId,
            OwnerId = userId,
            Title = "New task",
            Description = "  Description  . ",
            DueDate = dueDate,
        });

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal("New task", taskEntity.Title);
        Assert.Equal("Description  .", taskEntity.Description);
        Assert.Equal(dueDate, taskEntity.DueDate);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
