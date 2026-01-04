using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="CreateUserTaskAccessCommandHandler"/>.
/// Verifies that validation, user existence checks, task ownership,
/// and access creation behave correctly under different conditions.
/// </summary>
public class CreateUserTaskAccessCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<CreateUserTaskAccessCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateUserTaskAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("Email", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateUserTaskAccessCommandHandler(
            uowMock.Object,
            validatorMock.Object);

        var command = new CreateUserTaskAccessCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "test@test.com");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        uowMock.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Ensures that the handler returns a "NotFound" error when the user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        var validatorMock = ValidatorSuccess();

        var usersRepoMock = new Mock<IUserRepository>();
        usersRepoMock
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(usersRepoMock.Object);

        var handler = CreateHandler(uowMock, validatorMock);

        var result = await handler.HandleAsync(
            new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.NotFound, result.Error!.Code);
    }

    /// <summary>
    /// Ensures that the handler returns a validation error when the user is the owner of the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUserIsOwner()
    {
        var validatorMock = ValidatorSuccess();
        var user = new UserEntity("John", "john", "test@test.com", "hash");

        var userRepoMock = MockUsers(user);
        var taskRepoMock = MockTaskOwner(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(userRepoMock.Object);
        uowMock.Setup(t => t.Tasks).Returns(taskRepoMock.Object);

        var handler = CreateHandler(uowMock, validatorMock);

        var result = await handler.HandleAsync(
            new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error!.Code);
    }

    /// <summary>
    /// Ensures that the handler returns an invalid operation error when the user already has access.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnInvalidOperation_WhenUserAlreadyHasAccess()
    {
        var validatorMock = ValidatorSuccess();
        var user = new UserEntity("John", "john", "test@test.com", "hash");

        var userRepoMock = MockUsers(user);
        var taskRepoMock = MockTaskOwner(true);
        taskRepoMock
            .Setup(r => r.IsTaskOwnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var accessRepoMock = new Mock<IUserTaskAccessRepository>();
        accessRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(userRepoMock.Object);
        uowMock.Setup(t => t.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.UserTaskAccesses).Returns(accessRepoMock.Object);

        var handler = CreateHandler(uowMock, validatorMock);

        var result = await handler.HandleAsync(
            new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error!.Code);
    }

    /// <summary>
    /// Ensures that a user-task access is successfully created when all validation
    /// and ownership checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldCreateAccess_WhenAllChecksPass()
    {
        var validatorMock = ValidatorSuccess();
        var ownerId = Guid.NewGuid();
        var user = new UserEntity("John", "john", "test@test.com", "hash");
        var task = new TaskEntity(ownerId, Guid.NewGuid(), "test");

        var userRepoMock = MockUsers(user);

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.IsTaskOwnerAsync(task.Id, ownerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        taskRepoMock
            .Setup(r => r.IsTaskOwnerAsync(task.Id, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var accessRepoMock = new Mock<IUserTaskAccessRepository>();
        accessRepoMock
            .Setup(r => r.ExistsAsync(task.Id, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        accessRepoMock
            .Setup(r => r.AddAsync(It.IsAny<UserTaskAccessEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users).Returns(userRepoMock.Object);
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);
        uowMock.Setup(u => u.UserTaskAccesses).Returns(accessRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = CreateHandler(uowMock, validatorMock);

        var result = await handler.HandleAsync(
            new CreateUserTaskAccessCommand(task.Id, ownerId, user.Email),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        accessRepoMock.Verify(r => r.AddAsync(It.IsAny<UserTaskAccessEntity>(), It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Creates a validator mock that always succeeds.
    /// </summary>
    /// <returns>A <see cref="Mock{IValidator}"/> that returns a valid <see cref="ValidationResult"/>.</returns>
    private static Mock<IValidator<CreateUserTaskAccessCommand>> ValidatorSuccess()
    {
        var mock = new Mock<IValidator<CreateUserTaskAccessCommand>>();
        mock.Setup(v => v.ValidateAsync(It.IsAny<CreateUserTaskAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return mock;
    }

    /// <summary>
    /// Creates a mock <see cref="IUserRepository"/> that returns the given user.
    /// </summary>
    /// <param name="user">The user entity to return.</param>
    /// <returns>A <see cref="Mock{IUserRepository}"/> that returns the user.</returns>
    private static Mock<IUserRepository> MockUsers(UserEntity user)
    {
        var mock = new Mock<IUserRepository>();
        mock.Setup(r => r.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        return mock;
    }

    /// <summary>
    /// Creates a mock <see cref="ITaskRepository"/> that returns the specified ownership state.
    /// </summary>
    /// <param name="isOwner">Whether the user is considered the owner of the task.</param>
    /// <returns>A <see cref="Mock{ITaskRepository}"/> configured with the ownership state.</returns>
    private static Mock<ITaskRepository> MockTaskOwner(bool isOwner)
    {
        var mock = new Mock<ITaskRepository>();
        mock.Setup(r => r.IsTaskOwnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(isOwner);
        return mock;
    }

    /// <summary>
    /// Creates a <see cref="CreateUserTaskAccessCommandHandler"/> instance using the provided
    /// unit of work and validator mocks.
    /// </summary>
    /// <param name="uow">The unit of work mock.</param>
    /// <param name="validator">The validator mock.</param>
    /// <returns>A new <see cref="CreateUserTaskAccessCommandHandler"/> instance.</returns>
    private static CreateUserTaskAccessCommandHandler CreateHandler(
        Mock<IUnitOfWork> uow,
        Mock<IValidator<CreateUserTaskAccessCommand>> validator)
        => new(uow.Object, validator.Object);
}
