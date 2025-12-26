using FluentValidation;
using FluentValidation.Results;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteOverdueTasks;
using TodoListApp.Domain.Interfaces.Repositories;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteOverdueTasksCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteOverdueTasksCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails,
    /// and does not call repository or SaveChangesAsync.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<DeleteOverdueTasksCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteOverdueTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(
                [new ValidationFailure("TaskListId", "TaskList not found")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteOverdueTasksCommandHandler(uowMock.Object, validatorMock.Object);
        var command = new DeleteOverdueTasksCommand(Guid.NewGuid(), Guid.NewGuid());

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("TaskList not found", result.Error.Message);

        uowMock.Verify(
            u => u.Tasks.DeleteOverdueTasksAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the handler deletes overdue tasks successfully when validation passes,
    /// and calls DeleteOverdueTasksAsync and SaveChangesAsync exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteOverDueTasks_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<DeleteOverdueTasksCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteOverdueTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskRespository = new Mock<ITaskRepository>();
        taskRespository.Setup(r => r.DeleteOverdueTasksAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRespository.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteOverdueTasksCommandHandler(uowMock.Object, validatorMock.Object);
        var taskListId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new DeleteOverdueTasksCommand(taskListId, userId);

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        taskRespository.Verify(
            r => r.DeleteOverdueTasksAsync(
                userId,
                taskListId,
                It.Is<DateTime>(d => (DateTime.UtcNow - d).TotalSeconds < 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
