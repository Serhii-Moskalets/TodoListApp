using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Queries.GetTasks;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Enums;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTasksQueryHandler"/>.
/// Verifies validation handling, successful task retrieval,
/// and correct repository interaction with query parameters.
/// </summary>
public class GetTasksQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ITaskRepository> _taskRepositoryMock = new();
    private readonly Mock<IValidator<GetTasksQuery>> _validatorMock = new();

    /// <summary>
    /// Returns failure result when query validation fails
    /// and repository is not called.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var query = new GetTasksQuery(Guid.Empty, Guid.NewGuid());

        this._validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "User Id is required")]));

        var handler = this.CreateHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(TinyResult.Enums.ErrorCode.ValidationError, result.Error!.Code);

        this._taskRepositoryMock.Verify(
            r => r.GetTasksAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<StatusTask>?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<TaskSortBy?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Returns a successful result with tasks when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenValidationPasses()
    {
        // Arrange
        var query = new GetTasksQuery(Guid.NewGuid(), Guid.NewGuid());

        this._validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var entities = new List<TaskEntity>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Task title 1"),
            new(Guid.NewGuid(), Guid.NewGuid(), "Task title 2"),
        };

        this._taskRepositoryMock
            .Setup(r => r.GetTasksAsync(
                query.UserId,
                query.TaskListId,
                query.TaskStatuses,
                query.DueBefore,
                query.DueAfter,
                query.TaskSortBy,
                query.Ascending,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var handler = this.CreateHandler();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count());
    }

    /// <summary>
    /// Calls repository with exact query parameters when validation succeeds.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCallRepository_WithExactQueryParameters()
    {
        // Arrange
        var query = new GetTasksQuery(
            UserId: Guid.NewGuid(),
            TaskListId: Guid.NewGuid(),
            TaskStatuses: [StatusTask.NotStarted, StatusTask.InProgress],
            DueBefore: DateTime.UtcNow.AddDays(1),
            DueAfter: DateTime.UtcNow,
            TaskSortBy: TaskSortBy.Title,
            Ascending: false);

        this._validatorMock
            .Setup(v => v.ValidateAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        this._taskRepositoryMock
            .Setup(r => r.GetTasksAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<IReadOnlyCollection<StatusTask>?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<TaskSortBy>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = this.CreateHandler();

        // Act
        await handler.Handle(query, CancellationToken.None);

        // Assert
        this._taskRepositoryMock.Verify(
            r =>
            r.GetTasksAsync(
                query.UserId,
                query.TaskListId,
                query.TaskStatuses,
                query.DueBefore,
                query.DueAfter,
                query.TaskSortBy,
                query.Ascending,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private GetTasksQueryHandler CreateHandler()
    {
        this._unitOfWorkMock
            .Setup(x => x.Tasks)
            .Returns(this._taskRepositoryMock.Object);

        return new GetTasksQueryHandler(
            this._unitOfWorkMock.Object,
            this._validatorMock.Object);
    }
}
