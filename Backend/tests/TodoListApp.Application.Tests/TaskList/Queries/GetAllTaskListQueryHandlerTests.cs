using Moq;
using TodoListApp.Application.TaskList.Dtos;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllTaskListQueryHandler"/>.
/// Ensures that the handler correctly retrieves all task lists for a user and maps them to <see cref="TaskListDto"/>.
/// </summary>
public class GetAllTaskListQueryHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a list of <see cref="TaskListDto"/> when task lists exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskLists_WhenTaskListsExist()
    {
        var userId = Guid.NewGuid();
        var taskLists = new List<TaskListEntity>
        {
            new(userId, "Work"),
            new(userId, "Personal"),
        };

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(taskLists);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new GetAllTaskListQueryHandler(uowMock.Object);
        var query = new GetAllTaskListQuery(userId);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.Contains(result.Value, t => t.Title == "Work");
        Assert.Contains(result.Value, t => t.Title == "Personal");
    }

    /// <summary>
    /// Tests that the handler returns an empty list when the user has no task lists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTaskListsExist()
    {
        var userId = Guid.NewGuid();

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new GetAllTaskListQueryHandler(uowMock.Object);
        var query = new GetAllTaskListQuery(userId);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
