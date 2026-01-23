using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByTitleQueryHandler"/>.
/// Ensures that the handler correctly retrieves tasks by title for a specific user
/// and maps them to <see cref="TaskBriefDto"/> objects.
/// </summary>
public class GetTaskByTitleQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly GetTaskByTitleQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByTitleQueryHandlerTests"/> class.
    /// </summary>
    public GetTaskByTitleQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new GetTaskByTitleQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Verifies that the handler correctly calls the repository with provided filters
    /// and returns a paged result containing mapped <see cref="TaskBriefDto"/> objects.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDtos_WhenValidationPasses()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var text = "Task";
        var page = 1;
        var pageSize = 10;

        var taskEntities = new List<TaskEntity>
        {
            new(userId, Guid.NewGuid(), "Task 1", DateTime.UtcNow.AddDays(1)),
            new(userId, Guid.NewGuid(), "Task 2", DateTime.UtcNow.AddDays(2)),
        };

        this._taskRepoMock
            .Setup(r => r.SearchByTitleAsync(userId, text, page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync((taskEntities, taskEntities.Count));

        var query = new GetTaskByTitleQuery(userId, text, page, pageSize);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(taskEntities.Count, result.Value!.TotalCount);
        Assert.Equal(taskEntities.Count, result.Value.Items.Count);
        Assert.Equal(page, result.Value.Page);
        Assert.Equal("Task 1", result.Value.Items.First().Title);
    }

    /// <summary>
    /// Verifies that the handler returns an empty paged result without querying the database
    /// when the search text is null, empty, or consists only of whitespace.
    /// </summary>
    /// <param name="searchText">The search text to be tested, provided by <see cref="InlineDataAttribute"/>.</param>
    /// <remarks>
    /// This test ensures that the application performs early validation on the search input
    /// to prevent unnecessary repository calls and database load for invalid search queries.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenTextIsNullOrEmptyOrWhitespace(string? searchText)
    {
        // Arrange
        var query = new GetTaskByTitleQuery(Guid.NewGuid(), searchText, 1, 10);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0, result.Value.TotalCount);
        this._taskRepoMock.Verify(
            r =>
            r.SearchByTitleAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
