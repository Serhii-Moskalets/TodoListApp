using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByIdCommandValidator"/>.
/// Verifies validation rules for deleting a user-task access entry.
/// </summary>
public class DeleteTaskAccessByIdCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    /// <summary>
    /// Ensures that validation fails when the user-task access does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldHaveError_WhenAccessDoesNotExist()
    {
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var validator = new DeleteTaskAccessByIdCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors, e => e.PropertyName == "TaskId");
        Assert.Equal("User task access not found.", error.ErrorMessage);
    }

    /// <summary>
    /// Ensures that validation passes when the user-task access exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Validate_ShouldNotHaveError_WhenAccessExists()
    {
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(
                command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = new DeleteTaskAccessByIdCommandValidator(this._unitOfWorkMock.Object);

        var result = await validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }
}