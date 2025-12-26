using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByIdCommandHandler"/>.
/// Verifies behavior when deleting a user-task access by task and user IDs.
/// </summary>
public class DeleteTaskAccessByIdCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IValidator<DeleteTaskAccessByIdCommand>> _validatorMock = new();

    /// <summary>
    /// Ensures that the handler returns failure when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        this._validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "Invalid")]));

        var handler = new DeleteTaskAccessByIdCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler deletes the access and returns success when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenValidationPasses()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessByIdCommand(taskId, userId);

        this._validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                      .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.DeleteByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
                       .Returns(Task.CompletedTask);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        var handler = new DeleteTaskAccessByIdCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        this._unitOfWorkMock.Verify(u => u.UserTaskAccesses.DeleteByTaskAndUserIdAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}