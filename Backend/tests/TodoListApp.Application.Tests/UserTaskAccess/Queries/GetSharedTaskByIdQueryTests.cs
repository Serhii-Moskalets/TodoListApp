using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Mappers;
using TodoListApp.Application.UserTaskAccess.Queries.GetSharedTaskById;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Queries;

/// <summary>
/// Unit tests for <see cref="GetSharedTaskByIdQueryHandler"/>.
/// Verifies behavior when retrieving a shared task by its ID and user ID.
/// </summary>
public class GetSharedTaskByIdQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();
    private readonly Mock<IValidator<GetSharedTaskByIdQuery>> _validatorMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetSharedTaskByIdQueryHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public GetSharedTaskByIdQueryHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var query = new GetSharedTaskByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetSharedTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "TaskId is required.")]));

        var handler = new GetSharedTaskByIdQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("TaskId is required.", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler returns a failure result when the task does not exist for the given user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFound()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var query = new GetSharedTaskByIdQuery(taskId, userId);

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetSharedTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserTaskAccessEntity?)null);

        var handler = new GetSharedTaskByIdQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task not found.", result.Error!.Message);
    }

    /// <summary>
    /// Ensures that the handler returns the mapped task DTO when the task exists and is shared with the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDto_WhenTaskExists()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var taskEntity = new UserTaskAccessEntity(taskId, userId)
        {
            Task = new TaskEntity(userId, Guid.NewGuid(), "Test Task"),
            User = new UserEntity("John", "john", "john@example.com", "hash"),
        };

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetSharedTaskByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._userTaskAccessRepoMock
            .Setup(r => r.GetByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntity);

        var handler = new GetSharedTaskByIdQueryHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);
        var query = new GetSharedTaskByIdQuery(taskId, userId);

        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        var mapped = TaskAccessForUserMapper.Map(taskEntity);
        Assert.Equal(mapped.Id, result.Value!.Id);
    }
}
