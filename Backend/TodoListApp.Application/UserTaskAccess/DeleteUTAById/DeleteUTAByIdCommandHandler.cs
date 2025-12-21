using TinyResult;
using TodoListApp.Application.Abstractions.Messaging;
using TodoListApp.Domain.Interfaces.UnitOfWork;

namespace TodoListApp.Application.UserTaskAccess.DeleteUTAById;

/// <summary>
/// Handles the <see cref="DeleteUtaByIdCommand"/> to remove a user-task access entry.
/// </summary>
public class DeleteUtaByIdCommandHandler(IUnitOfWork unitOfWork)
    : HandlerBase(unitOfWork), ICommandHandler<DeleteUtaByIdCommand>
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
    public async Task<Result<bool>> Handle(DeleteUtaByIdCommand command, CancellationToken cancellationToken)
    {
        var exists = await this.UnitOfWork.UserTaskAccesses.ExistsAsync(command.TaskId, command.UserId, cancellationToken);

        if (!exists)
        {
            return await Result<bool>.FailureAsync(TinyResult.Enums.ErrorCode.NotFound, "User task access not found.");
        }

        await this.UnitOfWork.UserTaskAccesses.DeleteByTaskAndUserIdAsync(command.TaskId, command.UserId, cancellationToken);
        await this.UnitOfWork.SaveChangesAsync(cancellationToken);

        return await Result<bool>.SuccessAsync(true);
    }
}
