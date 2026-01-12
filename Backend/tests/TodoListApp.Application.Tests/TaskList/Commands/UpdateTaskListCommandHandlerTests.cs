using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Services;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Domain.Entities;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="UpdateTaskListCommandHandler"/>.
/// Verifies validation, existence checks, and title update logic when updating a task list.
/// </summary>
public class UpdateTaskListCommandHandlerTests
{
    /// <summary>
    /// Returns failure if validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("NewTitle", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();
        var serviceMock = new Mock<ITaskListNameUniquenessService>();

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object, validatorMock.Object);

        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Required", result.Error!.Message);
    }

    /// <summary>
    /// Returns failure if the task list does not exist.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTaskListDoesNotExist()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync((TaskListEntity)null!);

        var serviceMock = new Mock<ITaskListNameUniquenessService>();

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object, validatorMock.Object);

        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "NewTitle");
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task list not found.", result.Error!.Message);
    }

    /// <summary>
    /// Returns success without calling uniqueness service if the new title is unchanged.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenTitleIsUnchanged()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var taskList = new TaskListEntity(Guid.NewGuid(), "SameTitle");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(taskList);

        var serviceMock = new Mock<ITaskListNameUniquenessService>();

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object, validatorMock.Object);

        var command = new UpdateTaskListCommand(taskList.Id, Guid.NewGuid(), "SameTitle");
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        serviceMock.Verify(s => s.GetUniqueNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Updates the task list title if a new title is provided and ensures uniqueness.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTitle_WhenNewTitleIsDifferent()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new ValidationResult());

        var taskList = new TaskListEntity(Guid.NewGuid(), "OldTitle");

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists.GetTaskListByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(taskList);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var serviceMock = new Mock<ITaskListNameUniquenessService>();
        serviceMock.Setup(s => s.GetUniqueNameAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync((Guid userId, string newTitle, CancellationToken _) => newTitle + " Unique");

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, serviceMock.Object, validatorMock.Object);

        var command = new UpdateTaskListCommand(taskList.Id, Guid.NewGuid(), "NewTitle");
        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("NewTitle Unique", taskList.Title);
        serviceMock.Verify(s => s.GetUniqueNameAsync(It.IsAny<Guid>(), "NewTitle", It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
