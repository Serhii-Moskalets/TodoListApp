using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Queries.GetAllTaskList;

namespace TodoListApp.Application.Tests.TaskList.Queries;

/// <summary>
/// Unit tests for <see cref="GetAllTaskListQueryHandler"/>.
/// Verifies validation handling and retrieval of all task lists for a user.
/// </summary>
public class GetAllTaskListQueryHandlerTests
{
    /// <summary>
    /// Returns a failure result when query validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetAllTaskListQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllTaskListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("UserId", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new GetAllTaskListQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetAllTaskListQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Returns a collection of task list DTOs when validation passes.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskListDtos_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetAllTaskListQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetAllTaskListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var taskListEntities = new List<Domain.Entities.TaskListEntity>
        {
            new(Guid.NewGuid(), "Task 1"),
            new(Guid.NewGuid(), "Task 2"),
        };

        var taskListRepoMock = new Mock<ITaskListRepository>();
        taskListRepoMock
            .Setup(r => r.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListEntities);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRepoMock.Object);

        var handler = new GetAllTaskListQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetAllTaskListQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());

        Assert.Contains(result.Value, t => t.Title == "Task 1");
        Assert.Contains(result.Value, t => t.Title == "Task 2");
    }
}
