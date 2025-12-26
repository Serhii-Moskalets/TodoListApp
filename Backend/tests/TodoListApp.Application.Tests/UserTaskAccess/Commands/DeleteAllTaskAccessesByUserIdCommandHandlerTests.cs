using Moq;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteAllTaskAccessesByUserId;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.UserTaskAccess.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteAllTaskAccessesByUserIdCommandHandler"/>.
/// Verifies behavior for deleting all user-task access entries for a specific user.
/// </summary>
public class DeleteAllTaskAccessesByUserIdCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserTaskAccessRepository> _userTaskAccessRepoMock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteAllTaskAccessesByUserIdCommandHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteAllTaskAccessesByUserIdCommandHandlerTests()
    {
        this._unitOfWorkMock.Setup(u => u.UserTaskAccesses).Returns(this._userTaskAccessRepoMock.Object);
    }

    /// <summary>
    /// Tests that the handler deletes all user-task access entries for a given user
    /// and calls SaveChangesAsync exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteAllAccesses_WhenUserExists()
    {
        var userId = Guid.NewGuid();

        this._userTaskAccessRepoMock
            .Setup(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        this._unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteAllTaskAccessesByUserIdCommandHandler(this._unitOfWorkMock.Object);
        var command = new DeleteAllTaskAccessesByUserIdCommand(userId);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
