using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Tasks.Commands.ChangeTaskStatus;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;
using TodoListApp.Domain.Exceptions;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="ChangeTaskStatusCommandHandler"/>.
/// Verifies behavior when changing task status.
/// </summary>
public class ChangeTaskStatusCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns failure if the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new ChangeTaskStatusCommand(taskId, userId, Domain.Enums.StatusTask.InProgress);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new ChangeTaskStatusCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error?.Code);
        Assert.Equal("Task not found.", result.Error?.Message);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Ensures that the handler successfully changes the status of a task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldChangeStatusSuccessfully_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new ChangeTaskStatusCommand(taskId, userId, StatusTask.InProgress);

        var taskEntity = new TaskEntity(userId, Guid.NewGuid(), "Task");
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new ChangeTaskStatusCommandHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal(StatusTask.InProgress, taskEntity.Status);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
