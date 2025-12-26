using Moq;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetTaskWithSharedUsers;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

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
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTaskWithSharedUsersQuery(taskId, userId);

        this._tasksRepoMock.Setup(r => r.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

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
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetTaskWithSharedUsersQuery(taskId, userId);

        this._tasksRepoMock.Setup(r => r.IsTaskOwnerAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sharedAccessList = new List<UserTaskAccessEntity>
        {
            new(taskId, Guid.NewGuid())
            {
                User = new UserEntity("John1", "john1", "john1@example.com", "hash"),
                Task = new TaskEntity(userId, Guid.NewGuid(), "Task 1"),
            },
            new(taskId, Guid.NewGuid())
            {
                User = new UserEntity("John2", "john2", "john2@example.com", "hash"),
                Task = new TaskEntity(userId, Guid.NewGuid(), "Task 1"),
            },
        };

        this._userTaskAccessRepoMock.Setup(r => r.GetSharedTasksByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sharedAccessList);

        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        var mapped = TaskAccessForOwnerMapper.MapToTaskAccess(sharedAccessList);
        Assert.Contains(mapped.Users, u => u.Email == "john1@example.com");
        Assert.Contains(mapped.Users, u => u.Email == "john2@example.com");
    }
}
