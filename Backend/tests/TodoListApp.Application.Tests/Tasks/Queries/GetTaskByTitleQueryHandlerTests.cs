using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByTitleQueryHandler"/>.
/// Ensures that the handler correctly retrieves tasks by title for a specific user
/// and maps them to <see cref="TaskDto"/> objects.
/// </summary>
public class GetTaskByTitleQueryHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a list of <see cref="TaskDto"/> when tasks are found by title.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDtos_WhenTasksFound()
    {
        var userId = Guid.NewGuid();
        var queryText = "Test";
        var tasks = new List<TaskEntity>
        {
            new(userId, Guid.NewGuid(), "Test Task 1"),
            new(userId, Guid.NewGuid(), "Another Test Task"),
        };

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.SearchByTitleAsync(userId, queryText, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(tasks);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByTitleQueryHandler(uowMock.Object);
        var query = new GetTaskByTitleQuery(userId, queryText);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
        Assert.All(result.Value, t => Assert.Contains(queryText, t.Title, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tests that the handler returns an empty list when no tasks match the title.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTasksFound()
    {
        var userId = Guid.NewGuid();
        var queryText = "NonExistingTask";

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock.Setup(r => r.SearchByTitleAsync(userId, queryText, It.IsAny<CancellationToken>()))
                    .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByTitleQueryHandler(uowMock.Object);
        var query = new GetTaskByTitleQuery(userId, queryText);

        var ct = CancellationToken.None;
        var result = await handler.Handle(query, ct);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
