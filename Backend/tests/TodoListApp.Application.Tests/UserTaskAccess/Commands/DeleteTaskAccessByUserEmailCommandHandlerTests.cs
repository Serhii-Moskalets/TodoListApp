using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessByUserEmail;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByUserEmailCommandHandler"/>.
/// Verifies behavior for deleting a user-task access entry based on task ID and user email.
/// </summary>
public class DeleteTaskAccessByUserEmailCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidator<DeleteTaskAccessByUserEmailCommand>> _validatorMock = new();

    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Email", "Invalid email")]));

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid email", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a validation error when the user does not have access to the task.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnValidationError_WhenUserDoesNotHaveAccess()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com");

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(false);

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error!.Code);
        Assert.Equal("User doesn't have access to this task.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the user is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserNotFound()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@test.com");

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(command.Email!, It.IsAny<CancellationToken>()))
                       .ReturnsAsync((UserEntity?)null);

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("Opperation error.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when the delete operation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDeleteFails()
    {
        var user = new UserEntity("John", "john", "test@test.com", "hash");
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email);

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(0);

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("Access doesn't deleted.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler deletes the access successfully when all checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenAllChecksPass()
    {
        var user = new UserEntity("John", "john", "test@test.com", "hash");
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), user.Email);

        this._validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.Tasks.IsTaskOwnerAsync(command.TaskId, command.OwnerId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

        this._unitOfWorkMock.Setup(u => u.Users.GetByEmailAsync(user.Email, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(user);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        this._unitOfWorkMock.Verify(u => u.UserTaskAccesses.DeleteByIdAsync(command.TaskId, user.Id, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}