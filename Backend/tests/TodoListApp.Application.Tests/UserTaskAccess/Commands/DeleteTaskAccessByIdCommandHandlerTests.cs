using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskAccessByIdCommandHandler"/>.
/// Verifies behavior when deleting a user-task access by task and user IDs.
/// </summary>
public class DeleteTaskAccessByIdCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    /// <summary>
    /// Ensures that the handler returns failure when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var command = new DeleteTaskAccessByIdCommand(Guid.NewGuid(), Guid.NewGuid());

        var userTaskAccessRepoMock = new Mock<IUserTaskAccessRepository>();
        userTaskAccessRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(userTaskAccessRepoMock.Object);
        var handler = new DeleteTaskAccessByIdCommandHandler(this._unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("User hasn't accesss with this task.", result.Error.Message);
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

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.ExistsAsync(taskId, userId, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses.DeleteByIdAsync(taskId, userId, It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        this._unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                       .ReturnsAsync(1);

        var handler = new DeleteTaskAccessByIdCommandHandler(this._unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._unitOfWorkMock.Verify(u => u.UserTaskAccesses.DeleteByIdAsync(taskId, userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}