using Moq;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteTaskAccessesByUserCommandHandlerTests"/> class.
    /// Sets up repository mocks and configures the unit of work to return them.
    /// </summary>
    public DeleteTaskAccessesByUserCommandHandlerTests()
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
            .ReturnsAsync(1);

        this._unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteTaskAccessesByUserCommandHandler(this._unitOfWorkMock.Object);
        var command = new DeleteTaskAccessesByUserCommand(userId);

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);

        this._userTaskAccessRepoMock.Verify(r => r.DeleteAllByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        this._unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
