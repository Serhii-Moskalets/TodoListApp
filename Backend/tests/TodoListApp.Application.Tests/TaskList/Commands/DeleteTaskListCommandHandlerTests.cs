using FluentValidation;
using Moq;
using TodoListApp.Application.TaskList.Commands.DeleteTaskList;
using TodoListApp.Domain.Interfaces.Repositories;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.Tests.TaskList.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskListCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task lists.
/// </summary>
public class DeleteTaskListCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<DeleteTaskListCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<DeleteTaskListCommand>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("TaskListId", "Invalid Id")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTaskListCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskListCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.Handle(command, default);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid Id", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler deletes the task list successfully when validation passes.
    /// Ensures that DeleteAsync (from BaseRepository) and SaveChangesAsync are called exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShoudDeleteTaskList_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<DeleteTaskListCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskListCommand>(), default))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskListRespository = new Mock<ITaskListRepository>();
        taskListRespository.Setup(r => r.DeleteAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.TaskLists).Returns(taskListRespository.Object);
        uowMock.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var handler = new DeleteTaskListCommandHandler(uowMock.Object, validatorMock.Object);
        var taskListId = Guid.NewGuid();
        var command = new DeleteTaskListCommand(taskListId, Guid.NewGuid());

        var result = await handler.Handle(command, default);

        Assert.True(result.IsSuccess);
        taskListRespository.Verify(r => r.DeleteAsync(taskListId), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(default), Times.Once);
    }
}
