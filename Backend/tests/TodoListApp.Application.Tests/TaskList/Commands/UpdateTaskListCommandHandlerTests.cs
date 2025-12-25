using FluentValidation;
using Moq;
using TodoListApp.Application.TaskList.Commands.UpdateTaskList;
using TodoListApp.Domain.Entities;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Contains unit tests for <see cref="UpdateTaskListCommandHandler"/>.
/// Ensures that the command handler correctly validates, finds, and updates task lists.
/// </summary>
public class UpdateTaskListCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>[
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("Title", "Required")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), string.Empty);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Required", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler returns a failure result when the specified task list is not found.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenTaskListNotFound()
    {
        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskListRespository = new Mock<ITaskListRepository>();
        taskListRespository.Setup(r => r.GetByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskListEntity?)null);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRespository.Object);

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new UpdateTaskListCommand(Guid.NewGuid(), Guid.NewGuid(), "New Title");

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.Equal("Task list not found.", result.Error?.Message);
    }

    /// <summary>
    /// Tests that the handler updates the task list when validation passes and the task list exists.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldUpdateTaskList_WhenValidationPasses()
    {
        var userId = Guid.NewGuid();
        var taskListId = Guid.NewGuid();

        var validatorMock = new Mock<IValidator<UpdateTaskListCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<UpdateTaskListCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskListEntity = new TaskListEntity(userId, "Old Title");
        var taskListRespository = new Mock<ITaskListRepository>();
        taskListRespository.Setup(r => r.GetByIdForUserAsync(taskListId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskListEntity);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRespository.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateTaskListCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new UpdateTaskListCommand(taskListId, userId, "New Title");

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        Assert.Equal("New Title", taskListEntity.Title);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
