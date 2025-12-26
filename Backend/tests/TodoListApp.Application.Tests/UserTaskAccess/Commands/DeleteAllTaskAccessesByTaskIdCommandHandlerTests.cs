using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteAllTaskAccessesByTaskId;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteAllTaskAccessesByTaskIdCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific task.
/// </summary>
public class DeleteAllTaskAccessesByTaskIdCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITaskRepository> _tasksRepoMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAllTaskAccessesByTaskIdCommandHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteAllTaskAccessesByTaskIdCommandHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Tests that the handler returns a failure result when the task is not found
    /// or the user is not the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskNotFoundOrUserNotOwner()
    {
        var command = new DeleteAllTaskAccessesByTaskIdCommand(Guid.NewGuid(), Guid.NewGuid());

        this._tasksRepoMock
            .Setup(r => r.GetByIdAsync(command.TaskId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var handler = new DeleteAllTaskAccessesByTaskIdCommandHandler(this._unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task not found.", result.Error!.Message);

        var task = new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Test Task");
        this._tasksRepoMock.Setup(r => r.GetByIdAsync(command.TaskId, true, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(task);

        result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task not found.", result.Error!.Message);
    }

    /// <summary>
    /// Tests that the handler deletes all user-task access entries when the task exists
    /// and the user is the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenTaskExistsAndUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var task = new TaskEntity(userId, Guid.NewGuid(), "Task Title");

        this._tasksRepoMock.Setup(r => r.GetByIdAsync(taskId, true, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(task);

        this._userTaskAccessRepoMock.Setup(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()))
                               .Returns(Task.CompletedTask);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        var handler = new DeleteAllTaskAccessesByTaskIdCommandHandler(this._unitOfWorkMock.Object);
        var command = new DeleteAllTaskAccessesByTaskIdCommand(taskId, userId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByTaskIdAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}