using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByUser;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessesByUserCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific user.
/// </summary>
public class DeleteTaskAccessesByUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();
    private readonly Mock<IValidator<DeleteTaskAccessesByUserCommand>> _validatorMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByUserCommandHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteTaskAccessesByUserCommandHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessesByUserCommand(Guid.Empty);

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "UserId is required.")]));

        var handler = new DeleteTaskAccessesByUserCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("UserId is required.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler returns a failure result when there are no shared accesses for the user.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenNoAccessesExist()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessesByUserCommand(userId);

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new DeleteTaskAccessesByUserCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.InvalidOperation, result.Error!.Code);
        Assert.Equal("There are no tasks shared with you.", result.Error.Message);
    }

    /// <summary>
    /// Ensures the handler deletes all user-task accesses successfully when validation passes and accesses exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenValidationPassesAndAccessesExist()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessesByUserCommand(userId);

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessesByUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._userTaskAccessRepoMock
            .Setup(r => r.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        this._userTaskAccessRepoMock
            .Setup(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        this._unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteTaskAccessesByUserCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
