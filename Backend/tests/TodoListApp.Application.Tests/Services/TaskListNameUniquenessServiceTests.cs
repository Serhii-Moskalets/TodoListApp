using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Services;

namespace TodoListApp.Application.Tests.Services;

/// <summary>
/// Unit tests for <see cref="TaskListNameUniquenessService"/>.
/// Validates that unique task list names are generated correctly for a user.
/// </summary>
public class TaskListNameUniquenessServiceTests
{
    private const string Title = "Task List";

    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly TaskListNameUniquenessService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskListNameUniquenessServiceTests"/> class.
    /// Sets up mocks and the service under test.
    /// </summary>
    public TaskListNameUniquenessServiceTests()
    {
        this._unitOfWorkMock = new Mock<IUnitOfWork>();
        this._service = new TaskListNameUniquenessService(this._unitOfWorkMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListNameUniquenessService.GetUniqueNameAsync"/>
    /// returns the original title if no existing task list with the same title exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_ReturnsOriginalTitle_WhenTitleDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.Setup(x => x.TaskLists.ExistsByTitleAsync(Title, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await this._service.GetUniqueNameAsync(userId, Title, CancellationToken.None);

        // Assert
        Assert.Equal(Title, result);
    }

    /// <summary>
    /// Verifies that <see cref="TaskListNameUniquenessService.GetUniqueNameAsync"/>
    /// appends a numeric suffix to the title if the original title already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test execution.</returns>
    [Fact]
    public async Task GetUniqueNameAsync_AppendsSuffix_WhenTitleAlreadyExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        this._unitOfWorkMock.SetupSequence(x => x.TaskLists.ExistsByTitleAsync(It.IsAny<string>(), userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true) // "Task List" exists
                .ReturnsAsync(true) // "Task List (1)" exists
                .ReturnsAsync(false); // "Task List (2)" does not exists

        // Act
        var result = await this._service.GetUniqueNameAsync(userId, Title, CancellationToken.None);

        // Assert
        Assert.Equal("Task List (2)", result);
    }
}
