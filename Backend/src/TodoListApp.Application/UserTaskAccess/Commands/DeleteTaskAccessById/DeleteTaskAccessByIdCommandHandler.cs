using TinyResult;
using TodoListApp.Application.Abstractions.Interfaces.UnitOfWork;
using TodoListApp.Application.Abstractions.Messaging;

namespace TodoListApp.Application.UserTaskAccess.Commands.DeleteTaskAccessById;

/// <summary>
/// Handles the <see cref="DeleteTaskAccessByIdCommand"/> to remove a user-task access entry.
/// </summary>
public class DeleteTaskAccessByIdCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteTaskAccessByIdCommand, bool>
{
    /// <summary>
    /// Processes the command to delete a user-task access entry.
    /// </summary>
    /// <param name="command">
    /// The command containing the ID of the task and the user ID whose access should be removed.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> indicating success if the access was deleted,
    /// or failure if the access was not found.
    /// </returns>
    public async Task<Result<bool>> HandleAsync(DeleteTaskAccessByIdCommand command, CancellationToken cancellationToken)
    {
        if (!await this.HasAccess(command.TaskId, command.UserId, cancellationToken))
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.ValidationError, "User hasn't accesss with this task.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByIdAsync(command.TaskId, command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }

    /// <summary>
    /// Checks whether the specified user already has access to the task.
    /// </summary>
    /// <param name="taskId">The task ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
    /// <returns><c>true</c> if the user already has access; otherwise, <c>false</c>.</returns>
    private async Task<bool> HasAccess(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
       => await this.UnitOfWork.UserTaskAccesses.ExistsAsync(taskId, userId, cancellationToken);
}
