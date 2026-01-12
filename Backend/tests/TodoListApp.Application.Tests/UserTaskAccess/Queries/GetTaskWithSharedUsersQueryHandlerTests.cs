using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
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
    private readonly Mock<IValidator<GetTaskWithSharedUsersQuery>> _validatorMock = new();

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
    /// Ensures that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var query = new GetTaskWithSharedUsersQuery(Guid.Empty, Guid.NewGuid());

        this._validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "TaskId is required."),]));

        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("TaskId is required.", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result when the user is not the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNotTaskOwner()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, Guid.NewGuid(), "Task");
        var query = new GetTaskWithSharedUsersQuery(task.Id, otherUserId);

        this._validatorMock.Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, otherUserId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Task not found or you do not have permission.", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler returns the task with its shared users when the requesting user is the task owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSharedUsers_WhenUserIsTaskOwner()
    {
        var ownerId = Guid.NewGuid();
        var task = new TaskEntity(ownerId, Guid.NewGuid(), "Task");

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<GetTaskWithSharedUsersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(task.Id, ownerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var sharedAccessList = new List<UserTaskAccessEntity>
        {
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John1", "john1", "john1@example.com", "hash") },
            new(task.Id, Guid.NewGuid()) { User = new UserEntity("John2", "john2", "john2@example.com", "hash") },
        };

        this._userTaskAccessRepoMock.Setup(r => r.GetUserTaskAccessByTaskIdAsync(task.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sharedAccessList);

        var query = new GetTaskWithSharedUsersQuery(task.Id, ownerId);
        var handler = new GetTaskWithSharedUsersQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

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
