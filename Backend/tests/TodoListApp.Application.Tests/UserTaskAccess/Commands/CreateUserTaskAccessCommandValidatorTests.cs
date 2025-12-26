using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="CreateUserTaskAccessCommandValidator"/>.
/// Tests validation rules for creating user-task access.
/// </summary>
public class CreateUserTaskAccessCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    /// <summary>
    /// Tests that validation fails when the user already has access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserAlreadyHasAccess()
    {
        var command = new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid());

        var userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();
        userTaskAccessRepoMock
            .Setup(r => r.ExistsAsync(command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var tasksRepoMock = new Mock<ITaskRepository>();

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(userTaskAccessRepoMock.Object);
        this._unitOfWorkMock.Setup(u => u.Tasks).Returns(tasksRepoMock.Object);

        var validator = new CreateUserTaskAccessCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("User already has access to this task.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation fails when the user is the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenUserIsTaskOwner()
    {
        var command = new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new CreateUserTaskAccessCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("User is owner in this task.", error.ErrorMessage);
    }

    /// <summary>
    /// Tests that validation passes when the user does not have access and is not the owner.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenUserCanBeGrantedAccess()
    {
        var command = new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new CreateUserTaskAccessCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}
