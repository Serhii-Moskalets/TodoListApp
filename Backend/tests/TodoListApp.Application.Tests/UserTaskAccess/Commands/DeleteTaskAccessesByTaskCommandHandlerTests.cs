using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessesByTaskCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific task.
/// </summary>
public class DeleteTaskAccessesByTaskCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITaskRepository> _tasksRepoMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();
    private readonly Mock<IValidator<DeleteTaskAccessesByTaskCommand>> _validatorMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByTaskCommandHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteTaskAccessesByTaskCommandHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessesByTaskCommand(Guid.Empty, Guid.NewGuid());

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "TaskId is required.")]));

        var handler = new DeleteTaskAccessesByTaskCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("TaskId is required.", result.Error!.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the task does not exist
    /// or the user is not the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFoundOrUserNotOwner()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessesByTaskCommand(taskId, userId);

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByTaskCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync((TaskEntity?)null);

        var handler = new DeleteTaskAccessesByTaskCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("You do not have permission to delete accesses for this task.", result.Error!.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when no shared accesses exist for the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoSharedAccessesExist()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Test Task");
        var command = new DeleteTaskAccessesByTaskCommand(taskId, userId);

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByTaskCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(task);

        this._userTaskAccessRepoMock.Setup(r => r.ExistsByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(false);

        var handler = new DeleteTaskAccessesByTaskCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("There are no shared accesses for this task.", result.Error!.Message);
    }

    /// <summary>
    /// Ensures the handler deletes all user-task accesses successfully when the task exists and the user is the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenTaskExistsAndUserIsOwner()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Test Task");
        var command = new DeleteTaskAccessesByTaskCommand(taskId, userId);

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByTaskCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._tasksRepoMock.Setup(r => r.GetTaskByIdForUserAsync(taskId, userId, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(task);

        this._userTaskAccessRepoMock.Setup(r => r.ExistsByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

        this._userTaskAccessRepoMock.Setup(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(1);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        var handler = new DeleteTaskAccessesByTaskCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}