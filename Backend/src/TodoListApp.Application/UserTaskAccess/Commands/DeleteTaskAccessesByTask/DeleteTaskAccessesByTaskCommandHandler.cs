using TinyResult;
using TinyResult.Enums;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessesByTask;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessesByTaskCommand"/> to remove all user-task access entries for a specific task.
/// </summary>
public class DeleteTaskAccessesByTaskCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessesByTaskCommand, bool>
{
    /// <summary>
    /// Processes the command to delete all user-task access entries for a given task.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the task and the user ID of the task owner.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if all accesses were deleted,
    /// or failure if the task was not found or the user is not the task owner.
    /// </returns>
    public async Task<Result<bool>> Handle(DeleteTaskAccessesByTaskCommand command, CancellationToken cancellationToken)
    {
        var taskEntity = await this.UnitOfWork.Tasks.GetByIdAsync(command.TaskId, cancellationToken: cancellationToken);

        if (taskEntity is null)
        {
            return await Result<bool>.FailureAsync(ErrorCode.NotFound, "Task not found.");
        }

        if (taskEntity.OwnerId != command.UserId)
        {
            return await Result<bool>.FailureAsync(ErrorCode.ValidationError, "Only the task owner can delete accesses.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteAllByTaskIdAsync(command.TaskId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
