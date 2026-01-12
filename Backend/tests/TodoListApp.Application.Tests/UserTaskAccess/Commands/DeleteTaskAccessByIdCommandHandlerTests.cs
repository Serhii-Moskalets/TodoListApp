using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByIdCommandHandler"/>.
/// Verifies behavior when deleting a user-task access by task and user IDs.
/// </summary>
public class DeleteTaskAccessByIdCommandHandlerTests
{
    /// <summary>
    /// Ensures that the handler returns failure when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        var validatorMock = new Mock<IValidator<DeleteTaskAccessByIdCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("TaskId", "TaskId is required")]));

        var serviceMock = new Mock<IUserTaskAccessService>();
        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTaskAccessByIdCommandHandler(uowMock.Object, validatorMock.Object, serviceMock.Object);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("TaskId is required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler deletes the access and returns success when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenValidationPasses()
    {
        // Arrange
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        var validatorMock = new Mock<IValidator<DeleteTaskAccessByIdCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var serviceMock = new Mock<IUserTaskAccessService>();
        serviceMock
            .Setup(s => s.HasAccessAsync(command.TaskId, command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTaskAccessByIdCommandHandler(uowMock.Object, validatorMock.Object, serviceMock.Object);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCode.ValidationError, result.Error!.Code);
        Assert.Equal("User hasn't accesss with this task.", result.Error.Message);
    }

    /// <summary>
    /// Ensures that the handler deletes the access successfully when all validation and ownership checks pass.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAccess_WhenAllChecksPass()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteTaskAccessByIdCommand(taskId, userId);

        var validatorMock = new Mock<IValidator<DeleteTaskAccessByIdCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskAccessByIdCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var serviceMock = new Mock<IUserTaskAccessService>();
        serviceMock
            .Setup(s => s.HasAccessAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();
        userTaskAccessRepoMock
            .Setup(r => r.DeleteByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.UserTaskAccesses).Returns(userTaskAccessRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTaskAccessByIdCommandHandler(uowMock.Object, validatorMock.Object, serviceMock.Object);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        userTaskAccessRepoMock.Verify(r => r.DeleteByIdAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}