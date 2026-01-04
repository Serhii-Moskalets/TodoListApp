using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.Tasks.Commands.AddTagToTask;

/// <summary>
/// Handles the <see cref="AddTagToTaskCommand"/> by associating a tag with a specific task for a user.
/// </summary>
public class AddTagToTaskCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<AddTagToTaskCommand, bool>
{
    /// <summary>
    /// Handles the <see cref="AddTagToTaskCommand"/>.
    /// </summary>
    /// <param name="command">The command containing TaskId, UserId, and TagId.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the tag was added,
    /// or failure if the task was not found.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(AddTagToTaskCommand command, CancellationToken cancellationToken)
    {
        var taskEntity = await this.UnitOfWork.Tasks.GetByIdAsync(command.TaskId, false, cancellationToken);

        if (taskEntity is null || taskEntity.OwnerId != command.UserId)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "Task not found.");
        }

        taskEntity.SetTag(command.TagId);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
