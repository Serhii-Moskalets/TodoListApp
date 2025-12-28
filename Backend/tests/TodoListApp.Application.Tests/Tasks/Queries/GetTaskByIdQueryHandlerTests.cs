using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByIdQueryHandler"/>.
/// Ensures that the handler correctly retrieves a task and maps it to <see cref="TaskDto"/>.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when the task is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTaskByIdQuery(userId, taskId);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TaskEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByIdQueryHandler(uowMock.Object);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found.", result.Error.Message);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
    }

    /// <summary>
    /// Tests that the handler successfully returns a <see cref="TaskDto"/> when the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var taskEntity = new TaskEntity(userId, taskListId, "Test Task");
        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskEntity.Id, userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(taskEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByIdQueryHandler(uowMock.Object);

        var query = new GetTaskByIdQuery(userId, taskEntity.Id);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(taskEntity.Id, result.Value!.Id);
        Assert.Equal("Test Task", result.Value.Title);
        Assert.Equal(taskListId, result.Value.TaskListId);
        Assert.Equal(userId, result.Value.OwnerId);
        Assert.Equal(StatusTask.NotStarted, result.Value.Status);
    }
}
