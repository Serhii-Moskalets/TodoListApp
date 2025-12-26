using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTasksByUserId;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTasksByUserIdQueryHandler"/>.
/// Verifies behavior when retrieving all tasks shared with a specific user.
/// </summary>
public class GetSharedTasksByUserIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTasksByUserIdQueryHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public GetSharedTasksByUserIdQueryHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Tests that the handler returns an empty collection when the user has no shared tasks.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoSharedTasksExist()
    {
        var userId = Guid.NewGuid();
        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetSharedTasksByUserIdQueryHandler(this._unitOfWorkMock.Object);
        var query = new GetSharedTasksByUserIdQuery(userId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    /// <summary>
    /// Tests that the handler returns a collection of mapped task DTOs when shared tasks exist for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnMappedTaskDtos_WhenSharedTasksExist()
    {
        var userId = Guid.NewGuid();
        var task1 = new UserTaskAccessEntity(Guid.NewGuid(), userId)
        {
            Task = new TaskEntity(userId, Guid.NewGuid(), "Task 1"),
            User = new UserEntity("John1", "john1", "john1@example.com", "hash"),
        };
        var task2 = new UserTaskAccessEntity(Guid.NewGuid(), userId)
        {
            Task = new TaskEntity(userId, Guid.NewGuid(), "Task 2"),
            User = new UserEntity("John2", "john2", "john2@example.com", "hash"),
        };

        this._userTaskAccessRepoMock
            .Setup(r => r.GetSharedTasksByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([task1, task2]);

        var handler = new GetSharedTasksByUserIdQueryHandler(this._unitOfWorkMock.Object);
        var query = new GetSharedTasksByUserIdQuery(userId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var mappedTasks = TaskAccessForUserMapper.Map([task1, task2]);
        Assert.Equal(mappedTasks.Select(t => t.Id), result.Value!.Select(t => t.Id));
    }
}
