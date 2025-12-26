using Moq;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByUserEmailCommandValidator"/>.
/// Verifies validation rules for deleting a user-task access entry by user email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITaskRepository> _tasksRepoMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessByUserEmailCommandValidatorTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteTaskAccessByUserEmailCommandValidatorTests()
    {
        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(this._tasksRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Tests that validation fails when the user-task access does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenAccessDoesNotExist()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@example.com");

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsTaskAccessWithEmail(command.TaskId, command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new DeleteTaskAccessByUserEmailCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "Email");
        Assert.Equal("Task access is not found.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when the user is not the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserIsNotTaskOwner()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskAccessByUserEmailCommand(taskId, userId, "test@example.com");

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsTaskAccessWithEmail(taskId, command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._tasksRepoMock
            .Setup(r => r.GetByIdAsync(taskId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskEntity(Guid.NewGuid(), Guid.NewGuid(), "Task Title"));

        var validator = new DeleteTaskAccessByUserEmailCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("Only task owner can delete access.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the user-task access exists and the user is the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenAccessExistsAndUserIsOwner()
    {
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskAccessByUserEmailCommand(taskId, userId, "test@example.com");

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsTaskAccessWithEmail(taskId, command.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._tasksRepoMock
            .Setup(r => r.GetByIdAsync(taskId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskEntity(userId, Guid.NewGuid(), "Task Title"));

        var validator = new DeleteTaskAccessByUserEmailCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
