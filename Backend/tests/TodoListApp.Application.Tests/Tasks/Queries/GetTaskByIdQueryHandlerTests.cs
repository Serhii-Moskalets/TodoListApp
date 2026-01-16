using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.RemoveTagFromTask;
using TodoListApp.Application.Tasks.Queries.GetTaskById;
using TodoListApp.Application.Tests.Tasks.Commands;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByIdQueryHandler"/>.
/// Verifies the behavior of the handler for retrieving a task by its ID.
/// </summary>
public class GetTaskByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskRepository> _taskRepoMock;
    private readonly GetTaskByIdQueryHandler _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskByIdQueryHandlerTests"/> class.
    /// </summary>
    public GetTaskByIdQueryHandlerTests()
    {
        this._uowMock = new Mock<IUnitOfWork>();
        this._taskRepoMock = new Mock<ITaskRepository>();

        this._uowMock.Setup(u => u.Tasks).Returns(this._taskRepoMock.Object);
        this._handler = new GetTaskByIdQueryHandler(this._uowMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns NotFound when the task does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskDoesNotExist()
    {
        // Arrange
        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var query = new GetTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assertz
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
        Assert.Equal("Task not found.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a valid task DTO when the task exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Test Task", DateTime.UtcNow.AddDays(1));

        this._taskRepoMock
            .Setup(r => r.GetTaskByIdForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var query = new GetTaskByIdQuery(task.Id, userId);

        // Act
        var result = await this._handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(task.Id, result.Value!.Id);
        Assert.Equal(task.Title, result.Value.Title);
        Assert.Equal(task.Description, result.Value.Description);
        Assert.Equal(task.DueDate, result.Value.DueDate);
    }
}
