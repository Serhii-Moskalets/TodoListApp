using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteByTaskAccessByUserEmail;

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
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "test@example.com");

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
            [
                new ValidationFailure("Email", "Invalid email"),
            ]));

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid email", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler deletes the access successfully when validation passes.
    /// Ensures that DeleteByUserEmailAndTaskIdAsync and SaveChangesAsync are called exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenValidationPasses()
    {
        var command = new DeleteTaskAccessByUserEmailCommand(Guid.NewGuid(), Guid.NewGuid(), "user@example.com");

        this._validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByUserEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.DeleteByUserEmailAndTaskIdAsync(
                command.TaskId, command.Email, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteTaskAccessByUserEmailCommandHandler(this._unitOfWorkMock.Object, this._validatorMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._unitOfWorkMock.Verify(
            u => u.UserTaskAccesses.DeleteByUserEmailAndTaskIdAsync(
            command.TaskId, command.Email, It.IsAny<CancellationToken>()), Times.Once);

        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}