using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Common.Dtos;
using TodoListApp.Application.Tasks.Queries.GetTaskByTitle;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.Tasks.Queries;

/// <summary>
/// Unit tests for <see cref="GetTaskByTitleQueryHandler"/>.
/// Ensures that the handler correctly retrieves tasks by title for a specific user
/// and maps them to <see cref="TaskDto"/> objects.
/// </summary>
public class GetTaskByTitleQueryHandlerTests
{
    /// <summary>
    /// Returns failure result when query validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByTitleQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByTitleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Text", "Title is required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new GetTaskByTitleQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByTitleQuery(Guid.NewGuid(), string.Empty);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Title is required", result.Error.Message);
    }

    /// <summary>
    /// Returns mapped task DTOs when validation passes and tasks are found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnTaskDtos_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByTitleQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByTitleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var text = "Task";

        var taskEntities = new List<TaskEntity>
        {
            new(userId, Guid.NewGuid(), "Task 1", DateTime.UtcNow.AddDays(1)),
            new(userId, Guid.NewGuid(), "Task 2", DateTime.UtcNow.AddDays(2)),
        };

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.SearchByTitleAsync(userId, text, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskEntities);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByTitleQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByTitleQuery(userId, text);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count());

        // Перевірка мапінгу
        Assert.Contains(result.Value, t => t.Title == "Task 1");
        Assert.Contains(result.Value, t => t.Title == "Task 2");
    }

    /// <summary>
    /// Returns an empty collection when no tasks match the search criteria.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenNoTasksFound()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<GetTaskByTitleQuery>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<GetTaskByTitleQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var userId = Guid.NewGuid();
        var text = "NonExistent";

        var taskRepoMock = new Mock<ITaskRepository>();
        taskRepoMock
            .Setup(r => r.SearchByTitleAsync(userId, text, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRepoMock.Object);

        var handler = new GetTaskByTitleQueryHandler(uowMock.Object, validatorMock.Object);

        var query = new GetTaskByTitleQuery(userId, text);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }
}
