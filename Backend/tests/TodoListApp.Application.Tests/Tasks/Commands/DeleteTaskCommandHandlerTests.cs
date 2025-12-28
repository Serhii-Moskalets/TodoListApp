using FluentValidation;
using Moq;
using TodoListApp.Application.Abstractions.Interfaces.Repositories;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Tasks.Commands.DeleteTask;

namespace TodoListApp.Application.Tests.Tasks.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteTaskCommandHandler"/>.
/// Verifies the behavior of the handler for deleting task.
/// </summary>
public class DeleteTaskCommandHandlerTests
{
    /// <summary>
    /// Tests that the handler returns a failure result when validation fails.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenValidationFails()
    {
        var validatorMock = new Mock<IValidator<DeleteTaskCommand>>();
        validatorMock.Setup(x => x.ValidateAsync(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                [new FluentValidation.Results.ValidationFailure("TaskId", "Invalid Id")]));

        var uowMock = new Mock<IUnitOfWork>();

        var handler = new DeleteTaskCommandHandler(uowMock.Object, validatorMock.Object);

        var command = new DeleteTaskCommand(Guid.NewGuid(), Guid.NewGuid());

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal("Invalid Id", result.Error.Message);
    }

    /// <summary>
    /// Tests that the handler deletes the task successfully when validation passes.
    /// Ensures that DeleteAsync (from BaseRepository) and SaveChangesAsync are called exactly once.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Handle_ShouldDeleteTask_WhenValidationPasses()
    {
        var validatorMock = new Mock<IValidator<DeleteTaskCommand>>();
        validatorMock.Setup(v => v.ValidateAsync(It.IsAny<DeleteTaskCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var taskRespository = new Mock<ITaskRepository>();
        taskRespository.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var uowMock = new Mock<IUnitOfWork>();
        uowMock.Setup(u => u.Tasks).Returns(taskRespository.Object);
        uowMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteTaskCommandHandler(uowMock.Object, validatorMock.Object);
        var taskId = Guid.NewGuid();
        var command = new DeleteTaskCommand(taskId, Guid.NewGuid());

        var ct = CancellationToken.None;
        var result = await handler.Handle(command, ct);

        Assert.True(result.IsSuccess);
        taskRespository.Verify(r => r.DeleteAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
