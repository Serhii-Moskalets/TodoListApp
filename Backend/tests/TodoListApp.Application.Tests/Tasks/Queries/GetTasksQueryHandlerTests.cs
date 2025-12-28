using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTasks;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTasksQueryHandler"/>.
/// Ensures that the handler correctly retrieves tasks based on filters and maps them to <see cref="TaskDto"/>.
/// </summary>
public class GetTasksQueryHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a list of <see cref="TaskDto"/> when tasks exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTasks_WhenTasksExist()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task_1 = new TaskEntity(userId, taskListId, "Task 1");
        task_1.ChangeStatus(StatusTask.InProgress);
        task_1.ChangeStatus(StatusTask.Done);

        var task_2 = new TaskEntity(userId, taskListId, "Task 2");
        task_2.ChangeStatus(StatusTask.InProgress);

        var tasks = new List<TaskEntity>
        {
            task_1,
            task_2,
        };

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTasksAsync(
                userId,
                taskListId,
                null,
                null,
                null,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTasksQueryHandler(uowMock.Object);
        var query = new GetTasksQuery(userId, taskListId, null, null, null, null, true);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Contains(result.Value, t => t.Title == "Task 1");
        Assert.Contains(result.Value, t => t.Title == "Task 2");
    }

    /// <summary>
    /// Tests that the handler returns an empty list when no tasks match the filters.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTasksExist()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTasksAsync(
                userId,
                taskListId,
                null,
                null,
                null,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTasksQueryHandler(uowMock.Object);
        var query = new GetTasksQuery(userId, taskListId, null, null, null, null, true);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    /// <summary>
    /// Tests that the handler correctly applies status filter.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldFilterByStatus_WhenStatusProvided()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var task_1 = new TaskEntity(userId, taskListId, "Task 1");
        task_1.ChangeStatus(StatusTask.InProgress);
        task_1.ChangeStatus(StatusTask.Done);

        var task_2 = new TaskEntity(userId, taskListId, "Task 2");
        task_2.ChangeStatus(StatusTask.InProgress);

        var tasks = new List<TaskEntity>
        {
            task_1,
            task_2,
        };

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.GetTasksAsync(
                userId,
                taskListId,
                It.Is<IReadOnlyCollection<StatusTask>>(s => s!.Contains(StatusTask.Done)),
                null,
                null,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([.. tasks.Where(t => t.Status == StatusTask.Done)]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTasksQueryHandler(uowMock.Object);
        var query = new GetTasksQuery(
            userId,
            taskListId,
            [StatusTask.Done],
            null,
            null,
            null,
            true);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value);
        Assert.Equal(StatusTask.Done, result.Value!.First().Status);
    }
}
