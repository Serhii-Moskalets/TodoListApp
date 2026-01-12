using FluentValidation;
using FluentValidation.Results;
using Moq;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.CreateTaskList;
using TodoListApp.Domain.Entities;
using FVResult = FluentValidation.Results.ValidationResult;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="CreateTaskListCommandHandler"/>.
/// Verifies validation, user existence, and task list creation logic.
/// </summary>
public class CreateTaskListCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult([new ValidationFailure("Title", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var uniquenessServiceMock = new Mock<ITaskListNameUniquenessService>();

        var handler = new CreateTaskListCommandHandler(uowMock.Object, uniquenessServiceMock.Object, validatorMock.Object);

        var command = new CreateTaskListCommand(Guid.NewGuid(), string.Empty);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Returns NotFound if the specified user does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users.GetByIdAsync(userId, true, It.IsAny<CancellationToken>()))
               .ReturnsAsync((UserEntity)null!);

        var uniquenessServiceMock = new Mock<ITaskListNameUniquenessService>();

        var handler = new CreateTaskListCommandHandler(uowMock.Object, uniquenessServiceMock.Object, validatorMock.Object);

        var command = new CreateTaskListCommand(userId, "My Task List");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorCode.NotFound, result.Error.Code);
        Assert.Equal("User not found.", result.Error.Message);
    }

    /// <summary>
    /// Creates a task list successfully when validation passes and the user exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldCreateTaskList_WhenValidationPasses()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<CreateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new FVResult());

        var user = new UserEntity("John", "john", "john@example.com", "hash");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Users.GetByIdAsync(user.Id, true, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        uowMock.Setup(u => u.TaskLists.AddAsync(It.IsAny<TaskListEntity>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var uniquenessServiceMock = new Mock<ITaskListNameUniquenessService>();
        uniquenessServiceMock.Setup(s => s.GetUniqueNameAsync(user.Id, "My Task List", It.IsAny<CancellationToken>()))
                             .ReturnsAsync("My Task List");

        var handler = new CreateTaskListCommandHandler(uowMock.Object, uniquenessServiceMock.Object, validatorMock.Object);
        var command = new CreateTaskListCommand(user.Id, "My Task List");

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        uowMock.Verify(u => u.TaskLists.AddAsync(It.Is<TaskListEntity>(t => t.OwnerId == user.Id && t.Title == "My Task List"), It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
