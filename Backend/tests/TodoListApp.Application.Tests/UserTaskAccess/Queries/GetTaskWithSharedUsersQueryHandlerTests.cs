using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskWithSharedUsersQueryHandler"/>.
/// Verifies behavior when retrieving users with shared access to a task.
/// </summary>
public class GetTaskWithSharedUsersQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITaskRepository> _tasksRepoMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTaskWithSharedUsersQueryHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public GetTaskWithSharedUsersQueryHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Tests that the handler returns a failure result when the requesting user is not the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotTaskOwner()
    {
        var taskOwnerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var task = new TaskEntity(taskOwnerId, Guid.NewGuid(), "Task");
        var query = new GetTaskWithSharedUsersQuery(task.Id, requestingUserId);

        this._tasksRepoMock.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Only task owner can retrieve shared access.", result.Error!.Message);
    }

    /// <summary>
    /// Tests that the handler returns a list of users with shared access when the user is the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSharedUsers_WhenUserIsTaskOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, Guid.NewGuid(), "Task");

        this._tasksRepoMock.Setup(r => r.GetByIdAsync(task.Id))
            .ReturnsAsync(task);

        var sharedAccessList = new List<UserTaskAccessEntity>
        {
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John1", "john1", "john1@example.com", "hash") },
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John2", "john2", "john2@example.com", "hash") },
        };

        this._userTaskAccessRepoMock.Setup(r => r.GetUserTaskAccessByTaskIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sharedAccessList);

        var query = new GetTaskWithSharedUsersQuery(task.Id, ownerId);
        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(task.Id, result.Value!.Id);
        Assert.Equal("Task", result.Value.Title);
        Assert.Equal(2, result.Value.Users.Count);
        Assert.Contains(result.Value.Users, u => u.Email == "john1@example.com");
        Assert.Contains(result.Value.Users, u => u.Email == "john2@example.com");
    }
}
