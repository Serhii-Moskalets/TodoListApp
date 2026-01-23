using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Queries.GetTaskLists;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskListsQueryHandler"/>.
/// Verifies that the handler correctly retrieves and maps task list entities for a specific user.
/// </summary>
public class GetAllTaskListQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly GetTaskListsQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTaskListQueryHandlerTests"/> class.
    /// </summary>
    public GetAllTaskListQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new GetTaskListsQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="GetTaskListsQueryHandler.Handle"/> returns a successful result
    /// containing a collection of mapped DTOs when task lists exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnTaskListDtos_WhenListsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;
        var entities = new List<TaskListEntity>
        {
            new(userId, "List 1"),
            new(userId, "List 2"),
        };

        this._taskListRepoMock
            .Setup(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, entities.Count));

        var query = new GetTaskListsQuery(userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var resultItems = result.Value.Items.ToList();

        Assert.Equal(2, resultItems.Count);
        Assert.Equal(entities.Count, result.Value.TotalCount);
        Assert.Contains(resultItems, t => t.Title == "List 1");

        this._taskListRepoMock.Verify(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="GetTaskListsQueryHandler.Handle"/> returns a success result
    /// with an empty collection when the user has no task lists in the repository.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoListsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var page = 1;
        var pageSize = 10;

        this._taskListRepoMock
            .Setup(r => r.GetTaskListsAsync(userId, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<TaskListEntity>(), 0));

        var query = new GetTaskListsQuery(userId, page, pageSize);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }
}
