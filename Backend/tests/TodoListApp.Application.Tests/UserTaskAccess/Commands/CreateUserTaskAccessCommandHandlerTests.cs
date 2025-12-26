using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.CreateUserTaskAccess;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="CreateUserTaskAccessCommandHandler"/>.
/// Verifies validation handling and creation of user-task access.
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
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new CreateUserTaskAccessCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new CreateUserTaskAccessCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Ensures that a new user-task access is created successfully when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldAddUserTaskAccess_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<CreateUserTaskAccessCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreateUserTaskAccessCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();
        userTaskAccessRepoMock
            .Setup(r => r.AddAsync(It.IsAny<UserTaskAccessEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.UserTaskAccesses).Returns(userTaskAccessRepoMock.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateUserTaskAccessCommandHandler(uowMock.Object, validatorMock.Object);

        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new CreateUserTaskAccessCommand(taskId, userId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        userTaskAccessRepoMock.Verify(
            r => r.AddAsync(
                It.Is<UserTaskAccessEntity>(uta => uta.TaskId == taskId && uta.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
