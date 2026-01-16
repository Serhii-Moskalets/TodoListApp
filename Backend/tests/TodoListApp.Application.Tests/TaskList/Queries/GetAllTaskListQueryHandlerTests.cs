using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllTaskListQueryHandler"/>.
/// Verifies that the handler correctly retrieves and maps task list entities for a specific user.
/// </summary>
public class GetAllTaskListQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskListRepository> _taskListRepoMock;
    private readonly GetAllTaskListQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetAllTaskListQueryHandlerTests"/> class.
    /// </summary>
    public GetAllTaskListQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskListRepoMock = new Mock<ITaskListRepository>();

        this._uowMock.Setup(u => u.TaskLists).Returns(this._taskListRepoMock.Object);
        this._handler = new GetAllTaskListQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="GetAllTaskListQueryHandler.Handle"/> returns a successful result
    /// containing a collection of mapped DTOs when task lists exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnTaskListDtos_WhenListsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskListEntities = new List<TaskListEntity>
        {
            new(userId, "List 1"),
            new(userId, "List 2"),
        };

        this._taskListRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListEntities);

        var query = new GetAllTaskListQuery(userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var resultList = result.Value.ToList();

        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, t => t.Title == "List 1");
        Assert.Contains(resultList, t => t.Title == "List 2");

        this._taskListRepoMock.Verify(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="GetAllTaskListQueryHandler.Handle"/> returns a success result
    /// with an empty collection when the user has no task lists in the repository.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>[Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoListsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        this._taskListRepoMock
            .Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var query = new GetAllTaskListQuery(userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
    }
}
